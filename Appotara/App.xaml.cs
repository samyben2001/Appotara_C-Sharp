namespace Appotara
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            var window = base.CreateWindow(activationState);

            const int newWidth = 900;
            const int newHeight = 600;

            // Set new width/height to window
            window.MinimumWidth = window.MaximumWidth = window.Width = newWidth;
            window.MinimumHeight = window.MaximumHeight = window.Height = newHeight;

            // Get display size
            var displayInfo = DeviceDisplay.Current.MainDisplayInfo;

            // Center the window
            window.X = (displayInfo.Width / displayInfo.Density - window.Width) / 2;
            window.Y = (displayInfo.Height / displayInfo.Density - window.Height) / 2;


            return window;
        }
    }
}
