using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Terminal.Gui;
using App = Terminal.Gui.Application;
using TerminalAttribute = Terminal.Gui.Attribute;

namespace GameToolClaudeStyle
{
    class Program
    {
        // 1. Khai báo thư viện hệ thống Windows (API Win32) để nghe sự kiện từ Console
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate handler, bool add);

        private delegate bool ConsoleCtrlDelegate(CtrlTypes ctrlType);

        private enum CtrlTypes
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,   // Sự kiện khi người dùng bấm nút [X] hoặc Alt + F4
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        private static ConsoleCtrlDelegate _handler;

        private static Label dynamicTitleLabel;
        private static string currentTargetTitle = "";
        static string GetSecureRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            char[] stringChars = new char[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] data = new byte[length];
                rng.GetBytes(data);
                for (int i = 0; i < stringChars.Length; i++)
                {
                    stringChars[i] = chars[data[i] % chars.Length];
                }
            }
            return new string(stringChars);
        }

        static void Main(string[] args)
        {
            Console.Title = "Gacha Tool - code bởi YRF419       " + GetSecureRandomString(85);
            // Đăng ký hàm xử lý sự kiện đóng Console
            _handler = new ConsoleCtrlDelegate(OnConsoleClosing);
            SetConsoleCtrlHandler(_handler, true);
            bool ExitSDLL = System.IO.Directory.Exists("Dlls");
            if (!ExitSDLL)
            {
                System.IO.Directory.CreateDirectory("Dlls");
            }

            Console.InputEncoding = System.Text.Encoding.UTF8;
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            App.Init();
            var top = App.Top;

            // =======================================================
            // 🎨 THIẾT LẬP BẢNG MÀU PHẢN HỒI MẠNH
            // =======================================================
            var darkTheme = new ColorScheme()
            {
                Normal = TerminalAttribute.Make(Color.White, Color.Black),
                Focus = TerminalAttribute.Make(Color.BrightYellow, Color.DarkGray),
                HotNormal = TerminalAttribute.Make(Color.BrightCyan, Color.Black),
                HotFocus = TerminalAttribute.Make(Color.BrightCyan, Color.DarkGray)
            };

            var graySidebarTheme = new ColorScheme()
            {
                Normal = TerminalAttribute.Make(Color.Black, Color.Gray),
                Focus = TerminalAttribute.Make(Color.BrightCyan, Color.DarkGray),
                HotNormal = TerminalAttribute.Make(Color.DarkGray, Color.Gray),
                HotFocus = TerminalAttribute.Make(Color.BrightCyan, Color.DarkGray)
            };

            top.ColorScheme = darkTheme;

            // 1. Cửa sổ chính bao toàn màn hình (Sửa Height thành Dim.Fill())
            var mainWindow = new Window(" Gacha Tool - code bởi YRF419. Link Github https://github.com/FrySimpl3/Gacha ")
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(), // Đã sửa từ Dim.Fill() - 1 thành Dim.Fill() để chiếm trọn màn hình
                ColorScheme = darkTheme
            };
            top.Add(mainWindow);

            // =======================================================
            // CỘT TRÁI - SIDEBAR MENU
            // =======================================================
            var sidebar = new FrameView(" Menu Hệ Thống ")
            {
                X = 0,
                Y = 0,
                Width = Dim.Percent(25),
                Height = Dim.Fill(),
                ColorScheme = graySidebarTheme
            };

            string[] mainTabs = {
                " ►  Tải Launcher", "",
                " ►  Fix Lỗi Game",  "",
                " ►  Phần Mềm App",  "",
                " ►  Tải Game Tool"
            };

            var menuListView = new ListView(mainTabs)
            {
                X = 1,
                Y = 1,
                Width = Dim.Fill() - 1,
                Height = Dim.Fill() - 1,
                ColorScheme = graySidebarTheme
            };
            sidebar.Add(menuListView);
            mainWindow.Add(sidebar);

            // =======================================================
            // CỘT PHẢI - NỘI DUNG VÀ THAY ĐỔI CÁCH HIỂN THỊ NÚT
            // =======================================================
            var contentArea = new FrameView(" Chi Tiết Chức Năng (Bấm Tab để chọn nút) ")
            {
                X = Pos.Right(sidebar),
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                ColorScheme = darkTheme
            };
            mainWindow.Add(contentArea);

            dynamicTitleLabel = new Label("") { X = 2, Y = 1, ColorScheme = darkTheme };

            // --- View 1: Tải Launcher ---
            var viewLauncher = new View() { Width = Dim.Fill(), Height = Dim.Fill(), ColorScheme = darkTheme };
            viewLauncher.Add(new Label("Tải xuống Launcher tổng hợp để cài đặt tự động các game.") { X = 2, Y = 3 });
            var btnDlLauncher = new Button(" ► Bắt đầu tải Launcher ") { X = 2, Y = 5, ColorScheme = darkTheme };
            btnDlLauncher.Clicked += async () =>
            {
                string url = "https://github.com/FrySimpl3/Gacha_Tool_Console/releases/download/Gacha/GachaCity.exe";

                int result = MessageBox.Query("Xác nhận", "Bạn có muốn tải GachaCity.exe không?", "Có", "Hủy");
                if (result == 0)
                {
                    await FileDownloader.DownloadWithProgressAsync(url, "GachaCity.exe", true);
                }
                //Process.Start("GachaCity.exe");
            };
            viewLauncher.Add(btnDlLauncher);

            // --- View 2: Fix lỗi game ---
            var viewFixLoi = new View() { Width = Dim.Fill(), Height = Dim.Fill(), ColorScheme = darkTheme };
            var btnFixSocial = new Button(" ► Fix Social ") { X = 2, Y = 4, Width = 22, ColorScheme = darkTheme };
            var btnDlDriver = new Button(" ► Tải Driver ") { X = 2, Y = 6, Width = 22, ColorScheme = darkTheme };
            var btnCloseGacha = new Button(" ► Đóng Gacha ") { X = 2, Y = 8, Width = 22, ColorScheme = darkTheme };
            var btnToiUu = new Button(" ► Tối Ưu VGA ") { X = 2, Y = 10, Width = 22, ColorScheme = darkTheme };
            btnFixSocial.Clicked += async () =>
            {
                int result = MessageBox.Query("Xác nhận", "Bạn có muốn tải File không?", "Có", "Hủy");
                if (result == 0)
                {

                    await FileDownloader.DownloadWithProgressAsync("https://github.com/FrySimpl3/Gacha_Tool_Console/releases/download/V1.0.1/bink2w64.dll", "Dlls/bink2w64.dll");
                    await FileDownloader.DownloadWithProgressAsync("https://github.com/FrySimpl3/Gacha_Tool_Console/releases/download/V1.0.1/vcruntime140d.dll", "Dlls/vcruntime140d.dll");
                    await FileDownloader.DownloadWithProgressAsync("https://github.com/FrySimpl3/Gacha_Tool_Console/releases/download/V1.0.1/orig_socialclub.dll", "Dlls/orig_socialclub.dll");
                    await FileDownloader.DownloadWithProgressAsync("https://github.com/FrySimpl3/Gacha_Tool_Console/releases/download/V1.0.1/launc.dll", "Dlls/launc.dll");
                    Process.Start("explorer.exe", "Dlls");
                }
            };
            btnDlDriver.Clicked += async () =>
            {
                int result = MessageBox.Query("Xác nhận", "Bạn có muốn tải File không?", "Có", "Hủy");
                if (result == 0)
                {
                    await FileDownloader.DownloadWithProgressAsync("https://aka.ms/vc14/vc_redist.x64.exe", "vc_redist.x64.exe", true);
                }
            };
            btnCloseGacha.Clicked += async () =>
            {
                int result = MessageBox.Query("Xác nhận", "Bạn có muốn tải File không?", "Có", "Hủy");
                if (result == 0)
                {
                    ProcessManager.CloseGachaCity();
                }
            };

            btnToiUu.Clicked += async () =>
            {
                int result = MessageBox.Query("Xác nhận", "Bạn có muốn toi uu không?", "Có", "Hủy");
                if (result == 0)
                {
                    //ProcessManager.CloseGachaCity();
                    string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    string gtaSettingsPath = Path.Combine(appDataPath, "CitizenFX", "gta5_settings.xml");
                    //MessageBox.Query("Xác nhận", gtaSettingsPath, "Có", "Hủy");
                    string ContentConfig4GB = @"<?xml version=""1.0"" encoding=""UTF-8""?>

<Settings>
  <version value=""27"" />
  <configSource>SMC_AUTO</configSource>
  <graphics>
    <Tessellation value=""0"" />
    <LodScale value=""0.100000"" />
    <PedLodBias value=""0.200000"" />
    <VehicleLodBias value=""0.000000"" />
    <ShadowQuality value=""0"" />
    <ReflectionQuality value=""0"" />
    <ReflectionMSAA value=""0"" />
    <SSAO value=""0"" />
    <AnisotropicFiltering value=""0"" />
    <MSAA value=""0"" />
    <MSAAFragments value=""0"" />
    <MSAAQuality value=""0"" />
    <SamplingMode value=""0"" />
    <TextureQuality value=""0"" />
    <ParticleQuality value=""0"" />
    <WaterQuality value=""0"" />
    <GrassQuality value=""0"" />
    <ShaderQuality value=""0"" />
    <Shadow_SoftShadows value=""2"" />
    <UltraShadows_Enabled value=""false"" />
    <Shadow_ParticleShadows value=""true"" />
    <Shadow_Distance value=""1.000000"" />
    <Shadow_LongShadows value=""false"" />
    <Shadow_SplitZStart value=""0.930000"" />
    <Shadow_SplitZEnd value=""0.890000"" />
    <Shadow_aircraftExpWeight value=""0.990000"" />
    <Shadow_DisableScreenSizeCheck value=""false"" />
    <Reflection_MipBlur value=""true"" />
    <FXAA_Enabled value=""false"" />
    <TXAA_Enabled value=""false"" />
    <Lighting_FogVolumes value=""true"" />
    <Shader_SSA value=""false"" />
    <DX_Version value=""2"" />
    <CityDensity value=""0.200000"" />
    <PedVarietyMultiplier value=""0.200000"" />
    <VehicleVarietyMultiplier value=""0.200000"" />
    <PostFX value=""0"" />
    <DoF value=""false"" />
    <HdStreamingInFlight value=""false"" />
    <MaxLodScale value=""1.000000"" />
    <MotionBlurStrength value=""0.000000"" />
  </graphics>
  <system>
    <numBytesPerReplayBlock value=""9000000"" />
    <numReplayBlocks value=""30"" />
    <maxSizeOfStreamingReplay value=""1024"" />
    <maxFileStoreSize value=""65536"" />
  </system>
  <audio>
    <Audio3d value=""false"" />
  </audio>
  <video>
    <AdapterIndex value=""0"" />
    <OutputIndex value=""0"" />
    <ScreenWidth value=""20"" />
    <ScreenHeight value=""80"" />
    <RefreshRate value=""165"" />
    <Windowed value=""2"" />
    <VSync value=""1"" />
    <Stereo value=""0"" />
    <Convergence value=""0.100000"" />
    <Separation value=""1.000000"" />
    <PauseOnFocusLoss value=""1"" />
    <AspectRatio value=""0"" />
  </video>
  <VideoCardDescription>NVIDIA GeForce RTX 5060 Ti</VideoCardDescription>
</Settings>";
                    if (File.Exists(gtaSettingsPath))
                    {
                        string oldXmlContent = File.ReadAllText(gtaSettingsPath);
                        string updatedXml = ProcessManager.UpdateXmlSettings(ContentConfig4GB, oldXmlContent);
                        File.WriteAllText(gtaSettingsPath, updatedXml);
                    }
                }
            };
            viewFixLoi.Add(btnFixSocial, btnDlDriver, btnCloseGacha, btnToiUu);

            // --- View 3: Phần mềm ---
            var viewPhanMem = new View() { Width = Dim.Fill(), Height = Dim.Fill(), ColorScheme = darkTheme };
            var btnUltra = new Button(" ► Tải Tai Ultraview ") { X = 2, Y = 4, Width = 22, ColorScheme = darkTheme };
            var btnDiscord = new Button(" ► Tải ExpressVPN ") { X = 2, Y = 6, Width = 22, ColorScheme = darkTheme };

            btnDiscord.Clicked += async () =>
            {
                int result = MessageBox.Query("Xác nhận", "Bạn có muốn tải File không?", "Có", "Hủy");
                if (result == 0)
                {
                    await FileDownloader.DownloadWithProgressAsync("https://www.expressvpn.com/clients/windows/expressvpn-windows-x64-12.201.0.12153_release.exe", "expressvpn.exe", true);
                }
            };
            btnUltra.Clicked += async () =>
            {
                int result = MessageBox.Query("Xác nhận", "Bạn có muốn tải File không?", "Có", "Hủy");
                if (result == 0)
                {
                    await FileDownloader.DownloadWithProgressAsync("https://www.ultraviewer.net/vi/UltraViewer_setup_6.6_vi.exe", "UltraViewer_setup_6.exe", true);
                }
            };
            viewPhanMem.Add(btnUltra, btnDiscord);

            // --- View 4: Tải game ---
            var viewTaiGame = new View() { Width = Dim.Fill(), Height = Dim.Fill(), ColorScheme = darkTheme };
            var btnSteam = new Button(" ► Login Steam ") { X = 2, Y = 4, Width = 22, ColorScheme = darkTheme };
            var btnCmd = new Button(" ►  OFFLINE MODE ") { X = 2, Y = 6, Width = 22, ColorScheme = darkTheme };

            btnSteam.Clicked += async () =>
            {
                // 1. Danh sách tài khoản (Tuple gọn gàng)
                var accountList = new List<(string User, string Pass)>
                {
                    ("ieIko77539", "gacha@123"),
                    ("Dvc123465", "CONG123465a"),
                    ("4gamerz744", "80804000"),
                    ("gamebanquyen_1272", "Ducanh@123")
                };

                // 2. Chuyển đổi sang mảng NStack.ustring để không bị lỗi Type
                var userNames = new NStack.ustring[accountList.Count];
                for (int i = 0; i < accountList.Count; i++)
                {
                    //userNames[i] = NStack.ustring.Make($" [{i + 1}] Tài khoản: {accountList[i].User}");
                    userNames[i] = NStack.ustring.Make($" Tài khoản Số {i + 1} ");
                }

                // 3. Tạo một cửa sổ Dialog nhỏ làm màn hình chọn
                var loginDialog = new Dialog(" CLICK HOẶC DI CHUYỂN ĐỂ ĐĂNG NHẬP ", 55, 10)
                {
                    ColorScheme = darkTheme
                };

                // Giao diện chọn Radio
                var radioGroup = new RadioGroup(userNames)
                {
                    X = 2,
                    Y = 1,
                    SelectedItem = -1 // Để mặc định không chọn cái nào, bắt buộc user phải tương tác mới kích hoạt
                };

                // ⚡ SỰ KIỆN: Chọn phát đăng nhập luôn
                radioGroup.SelectedItemChanged += (e) =>
                {
                    // e.SelectedItem là vị trí tài khoản vừa được chọn (0, 1, 2...)
                    if (e.SelectedItem >= 0 && e.SelectedItem < accountList.Count)
                    {
                        var selectedAccount = accountList[e.SelectedItem];

                        // Đóng cửa sổ chọn tài khoản ngay lập tức
                        App.RequestStop();

                        // Tiến hành kích hoạt tiến trình Steam
                        SteamProcessController.LoginToSteamApp(selectedAccount.User, selectedAccount.Pass);
                    }
                };

                loginDialog.Add(radioGroup);

                // Chỉ giữ lại duy nhất nút Hủy nếu người dùng muốn thoát ra menu chính
                var btnCancel = new Button("Hủy Bỏ");
                btnCancel.Clicked += () => App.RequestStop();
                loginDialog.AddButton(btnCancel);

                // Chạy hộp thoại
                App.Run(loginDialog);
            };
            btnCmd.Clicked += async () =>
            {
                int result = MessageBox.Query("Xác nhận", "Bạn có muốn OFfline Mod không?", "Có", "Hủy");
                if (result == 0)
                {
                    SteamProcessController.OfflineMod();
                }
            };
            viewTaiGame.Add(btnSteam, btnCmd);

            // =======================================================
            // LOGIC ĐIỀU HƯỚNG
            // =======================================================
            Action updateContent = () =>
            {
                contentArea.RemoveAll();
                contentArea.Add(dynamicTitleLabel);

                switch (menuListView.SelectedItem)
                {
                    case 0: currentTargetTitle = " TRÌNH KHỞI ĐỘNG GAME CHÍNH THỨC"; contentArea.Add(viewLauncher); break;
                    case 2: currentTargetTitle = " TRÌNH SỬA LỖI & TỐI ƯU HỆ THỐNG"; contentArea.Add(viewFixLoi); break;
                    case 4: currentTargetTitle = " PHẦN MỀM VÀ CÔNG CỤ BỔ TRỢ"; contentArea.Add(viewPhanMem); break;
                    case 6: currentTargetTitle = " HỖ TRỢ TẢI GAME & TÀI KHOẢN"; contentArea.Add(viewTaiGame); break;
                }
                TriggerTypewriterEffect(currentTargetTitle);
            };

            int lastSelectedIndex = 0;
            menuListView.SelectedItemChanged += (e) =>
            {
                if (menuListView.SelectedItem % 2 != 0)
                {
                    if (menuListView.SelectedItem > lastSelectedIndex) { if (menuListView.SelectedItem < mainTabs.Length - 1) menuListView.SelectedItem++; else menuListView.SelectedItem--; }
                    else { if (menuListView.SelectedItem > 0) menuListView.SelectedItem--; else menuListView.SelectedItem++; }
                }
                lastSelectedIndex = menuListView.SelectedItem;
                updateContent();
            };

            updateContent();

            // Đã xóa hoàn toàn đoạn khởi tạo và top.Add(statusBar) ở vị trí này.

            App.Run();
            App.Shutdown();
        }

        private static async void TriggerTypewriterEffect(string fullText)
        {
            dynamicTitleLabel.Text = "";
            for (int i = 0; i <= fullText.Length; i++)
            {
                if (currentTargetTitle != fullText) return;
                dynamicTitleLabel.Text = fullText.Substring(0, i);
                App.MainLoop.Invoke(() => { dynamicTitleLabel.SetNeedsDisplay(); });
                await Task.Delay(20);
            }
        }

        /// <summary>
        /// Hàm này sẽ TỰ ĐỘNG CHẠY KHI NGƯỜI DÙNG ĐÓNG CONSOLE
        /// </summary>
        private static bool OnConsoleClosing(CtrlTypes ctrlType)
        {
            switch (ctrlType)
            {
                case CtrlTypes.CTRL_CLOSE_EVENT:
                case CtrlTypes.CTRL_LOGOFF_EVENT:
                case CtrlTypes.CTRL_SHUTDOWN_EVENT:

                    // Thực hiện tắt ngay Steam khi Console bị đóng
                    //SteamProcessController.KillSteamProcess();

                    break;
            }
            return true; // Trả về true để cho phép Windows đóng Console bình thường
        }
    }
}