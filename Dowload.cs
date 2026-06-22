using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Terminal.Gui;
using App = Terminal.Gui.Application;

namespace GameToolClaudeStyle
{
    public static class FileDownloader
    {
        private static readonly HttpClient httpClient = new HttpClient();

        /// <summary>
        /// Tải một file từ URL, hiển thị Progress Bar trên giao diện Terminal.Gui
        /// </summary>
        /// <param name="url">Đường dẫn tải file</param>
        /// <param name="destinationPath">Tên file hoặc đường dẫn lưu file</param>
        /// <param name="openAfterDownload">Tự động mở file sau khi tải xong nếu truyền true</param>
        public static async Task DownloadWithProgressAsync(string url, string destinationPath, bool openAfterDownload = false)
        {
            // 1. Khởi tạo giao diện hiển thị tiến trình
            var progressDialog = new Dialog("Đang tải xuống...", 55, 7);
            var statusLabel = new Label("Đang kết nối đến máy chủ...") { X = Pos.Center(), Y = 1 };
            var progressBar = new ProgressBar() { X = Pos.Center(), Y = 3, Width = 45 };

            progressDialog.Add(statusLabel, progressBar);

            // Chạy dialog đè lên giao diện hiện tại
            App.MainLoop.Invoke(() => { App.Run(progressDialog); });

            bool isSuccess = false;

            try
            {
                // 2. Gọi API và đọc dữ liệu theo Stream để tối ưu bộ nhớ
                using (var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();
                    var totalBytes = response.Content.Headers.ContentLength;

                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        var buffer = new byte[8192];
                        long totalReadBytes = 0;
                        int readBytes;

                        while ((readBytes = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, readBytes);
                            totalReadBytes += readBytes;

                            // Cập nhật phần trăm lên giao diện
                            if (totalBytes.HasValue)
                            {
                                float progress = (float)totalReadBytes / totalBytes.Value;
                                App.MainLoop.Invoke(() => {
                                    progressBar.Fraction = progress;
                                    statusLabel.Text = $"Đang tải: {(progress * 100):F1}% ({(totalReadBytes / 1024 / 1024):F1}MB / {(totalBytes.Value / 1024 / 1024):F1}MB)";
                                });
                            }
                        }
                    }
                }

                isSuccess = true;
                // Đóng Progress Dialog khi hoàn thành
                App.MainLoop.Invoke(() => { progressDialog.Running = false; });
            }
            catch (Exception ex)
            {
                // Đóng Progress Dialog nếu có lỗi xảy ra
                App.MainLoop.Invoke(() => { progressDialog.Running = false; });
                MessageBox.Query("Lỗi tải file", $"Không thể tải file. Chi tiết:\n{ex.Message}", "OK");
            }

            // 3. Xử lý mở file sau khi Dialog tiến trình đã đóng hoàn toàn
            if (isSuccess && openAfterDownload)
            {
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = destinationPath,
                        UseShellExecute = true // Bắt buộc phải là true để chạy file bằng ứng dụng mặc định của OS
                    };
                    Process.Start(psi);
                }
                catch (Exception ex)
                {
                    MessageBox.Query("Lỗi mở file", $"Không thể mở file tự động. Chi tiết:\n{ex.Message}", "OK");
                }
            }
        }
    }
}