using System;
using System.Diagnostics;
using System.IO;

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
                Console.WriteLine("Không tìm thấy tiến trình GachaCity.exe nào đang chạy.");
                return;
            }

            foreach (Process process in processes)
            {
                Console.WriteLine($"Đang đóng tiến trình: {process.ProcessName} (PID: {process.Id})...");

                // Gửi yêu cầu đóng giao diện (tắt một cách an toàn)
                process.CloseMainWindow();

                // Chờ tối đa 3 giây để tiến trình tự đóng
                if (!process.WaitForExit(3000))
                {
                    // Nếu không tự đóng sau 3 giây, ép buộc hủy (Kill)
                    Console.WriteLine("Tiến trình không phản hồi, đang ép buộc dừng (Kill)...");
                    process.Kill();
                }

                Console.WriteLine("Đã đóng thành công.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Có lỗi xảy ra khi đóng tiến trình: {ex.Message}");
        }
    }
    public static void StartProcess(string filePath, string arguments = "")
    {
        try
        {
            // Kiểm tra xem file exe có tồn tại thực sự hay không
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Lỗi: Không tìm thấy file tại đường dẫn: {filePath}");
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

            Console.WriteLine($"Đang khởi chạy: {Path.GetFileName(filePath)}...");

            // Chạy file
            Process.Start(startInfo);

            Console.WriteLine("Khởi chạy thành công!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Có lỗi xảy ra khi chạy file: {ex.Message}");
        }
    }
}