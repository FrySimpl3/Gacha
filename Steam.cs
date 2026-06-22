using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace GameToolClaudeStyle
{
    public static class SteamProcessController
    {
        /// <summary>
        /// Tự động tìm đường dẫn cài đặt của Steam.exe trên máy tính thông qua Registry
        /// </summary>
        public static string GetSteamPath()
        {
            // Kiểm tra Registry của Windows để tìm nơi cài đặt Steam
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam"))
            {
                if (key != null)
                {
                    string path = key.GetValue("SteamExe")?.ToString();
                    if (!string.IsNullOrEmpty(path) && File.Exists(path))
                    {
                        return path;
                    }
                }
            }

            // Nếu không thấy trong Registry, thử tìm ở các đường dẫn mặc định
            string default64 = @"C:\Program Files (x86)\Steam\Steam.exe";
            if (File.Exists(default64)) return default64;

            string default32 = @"C:\Program Files\Steam\Steam.exe";
            if (File.Exists(default32)) return default32;

            return null;
        }

        /// <summary>
        /// Tắt hoàn toàn tiến trình Steam đang chạy trên máy
        /// </summary>
        public static void KillSteamProcess()
        {
            Process[] processes = Process.GetProcessesByName("Steam");
            foreach (var process in processes)
            {
                try
                {
                    process.Kill();
                    process.WaitForExit(3000);
                }
                catch { /* Bỏ qua nếu không có quyền */ }
            }
        }

        /// <summary>
        /// Đăng nhập tài khoản trực tiếp vào phần mềm Steam.exe trên máy tính
        /// </summary>
        public static bool LoginToSteamApp(string username, string password)
        {
            string steamPath = GetSteamPath();
            if (string.IsNullOrEmpty(steamPath))
            {
                Console.WriteLine("Lỗi: Không tìm thấy phần mềm Steam cài đặt trên máy tính này.");
                return false;
            }

            // Bước 1: Nên tắt Steam đang chạy ngầm trước để tránh xung đột tài khoản cũ
            KillSteamProcess();

            try
            {
                // Bước 2: Khởi chạy Steam với các tham số đăng nhập tự động
                var startInfo = new ProcessStartInfo
                {
                    FileName = steamPath,
                    // -login [user] [pass]: Lệnh ép Steam đăng nhập tài khoản này
                    // -silent: Mở ẩn dưới khay hệ thống (tùy chọn)
                    Arguments = $"-login \"{username}\" \"{password}\" -offlinemode -url steam://open/games",
                    UseShellExecute = true
                };

                Process.Start(startInfo);
                Console.WriteLine($"Đang kích hoạt Steam.exe để đăng nhập tài khoản: {username}...");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi khởi chạy Steam: {ex.Message}");
                return false;
            }
        }

        public static bool OfflineMod()
        {
            string steamPath = GetSteamPath();
            if (string.IsNullOrEmpty(steamPath))
            {
                Console.WriteLine("Lỗi: Không tìm thấy phần mềm Steam cài đặt trên máy tính này.");
                return false;
            }
            try
            {
                var offlineInfo = new ProcessStartInfo
                {
                    FileName = steamPath,
                    Arguments = "-offline", // Lệnh native của Steam để ép sang Offline
                    UseShellExecute = true
                };
                Process.Start(offlineInfo);

                Console.WriteLine($"Đang kích hoạt Steam.exe để đăng nhập tài khoản: ...");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi khởi chạy Steam: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Lệnh khởi chạy nhanh một Game trên Steam bằng ID (AppID)
        /// </summary>
        public static void LaunchGame(string appId)
        {
            // Steam hỗ trợ giao thức steam:// mở nhanh game qua trình duyệt/hệ thống
            try
            {
                Process.Start($"steam://rungameid/{appId}");
                Console.WriteLine($"Đang gửi lệnh mở Game có ID: {appId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Không thể mở game: {ex.Message}");
            }
        }
    }
}