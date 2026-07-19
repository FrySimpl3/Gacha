using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;

public class ProcessManager
{
    public static void CloseGachaCity()
    {
        // Tên tiến trình không bao gồm đuôi .exe
        string processName = "GachaCity";

        try
        {
            // Lấy tất cả các tiến trình đang chạy có tên GachaCity
            Process[] processes = Process.GetProcessesByName(processName);

            if (processes.Length == 0)
            {
                //Console.WriteLine("Không tìm thấy tiến trình GachaCity.exe nào đang chạy.");
                return;
            }

            foreach (Process process in processes)
            {
                //Console.WriteLine($"Đang đóng tiến trình: {process.ProcessName} (PID: {process.Id})...");

                // Gửi yêu cầu đóng giao diện (tắt một cách an toàn)
                process.CloseMainWindow();

                // Chờ tối đa 3 giây để tiến trình tự đóng
                if (!process.WaitForExit(3000))
                {
                    // Nếu không tự đóng sau 3 giây, ép buộc hủy (Kill)
                    //Console.WriteLine("Tiến trình không phản hồi, đang ép buộc dừng (Kill)...");
                    process.Kill();
                }

                //Console.WriteLine("Đã đóng thành công.");
            }
        }
        catch (Exception ex)
        {
            //Console.WriteLine($"Có lỗi xảy ra khi đóng tiến trình: {ex.Message}");
        }
    }
    public static void StartProcess(string filePath, string arguments = "")
    {
        try
        {
            // Kiểm tra xem file exe có tồn tại thực sự hay không
            if (!File.Exists(filePath))
            {
                //Console.WriteLine($"Lỗi: Không tìm thấy file tại đường dẫn: {filePath}");
                return;
            }

            // Cấu hình thông tin để khởi chạy tiến trình
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = filePath,        // Đường dẫn tới file .exe
                Arguments = arguments,      // Tham số truyền vào nếu có (ví dụ: "-fullscreen")
                UseShellExecute = true,     // Sử dụng shell của hệ điều hành để chạy
                WorkingDirectory = Path.GetDirectoryName(filePath) // Đặt thư mục làm việc là thư mục chứa file (tránh lỗi thiếu file đi kèm)
            };

            //Console.WriteLine($"Đang khởi chạy: {Path.GetFileName(filePath)}...");

            // Chạy file
            Process.Start(startInfo);

            //Console.WriteLine("Khởi chạy thành công!");
        }
        catch (Exception ex)
        {
            //Console.WriteLine($"Có lỗi xảy ra khi chạy file: {ex.Message}");
        }
    }

    /// <summary>
    /// Chỉnh sửa văn bản bên ngoài thẻ <VideoCardDescription>, giữ nguyên nội dung bên trong.
    /// </summary>
    /// <param name="inputText">Toàn bộ nội dung file text đầu vào</param>
    /// <returns>Chuỗi text sau khi đã chỉnh sửa</returns>
    public static string UpdateXmlSettings(string newXmlContent, string oldXmlContent)
    {
        try
        {
            // 1. Dùng Regex để tìm và lấy CHÍNH XÁC giá trị nằm GIỮA cặp thẻ của file cũ
            // Pattern này quét không phân biệt chữ hoa thường (?i) và nhận diện mọi ký tự bao gồm xuống dòng
            Match matchOld = Regex.Match(oldXmlContent, @"<VideoCardDescription>([\s\S]*?)<\/VideoCardDescription>", RegexOptions.IgnoreCase);

            string videoCardValue = "";

            if (matchOld.Success)
            {
                // Lấy phần text nằm trong thẻ (Group 1)
                videoCardValue = matchOld.Groups[1].Value;
            }
            else
            {
                // Dự phòng: Nếu game tự viết tắt thành dạng tự đóng <VideoCardDescription /> khi lỗi
                // thì ta tìm dạng thẻ tự đóng này
                Match matchSelfClosing = Regex.Match(oldXmlContent, @"<VideoCardDescription\s*\/>", RegexOptions.IgnoreCase);
                if (!matchSelfClosing.Success)
                {
                    // Nếu file cũ hoàn toàn không có bất kỳ dấu vết nào của thẻ card đồ họa, trả về file mới nguyên bản
                    return newXmlContent;
                }
            }

            // 2. Định hình cụm thẻ XML hoàn chỉnh để chuẩn bị chèn vào file mới
            string newTagToInsert = $"\n  <VideoCardDescription>{videoCardValue}</VideoCardDescription>";

            // 3. Tiến hành chèn vào file mới (ContentConfig4GB của bạn)
            // Vì file mới của bạn chắc chắn không có thẻ này, ta sẽ chèn nó vào ngay trước thẻ đóng </Settings>
            if (newXmlContent.Contains("</Settings>"))
            {
                newXmlContent = newXmlContent.Replace("</Settings>", newTagToInsert + "\n</Settings>");
            }

            return newXmlContent;
        }
        catch (Exception ex)
        {
            // Nếu có lỗi bất ngờ, trả về cấu hình mới để tránh làm trống file của user
            Console.WriteLine("Lỗi trích xuất VideoCardDescription: " + ex.Message);
            return newXmlContent;
        }
    }
}