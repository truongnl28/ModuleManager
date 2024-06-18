namespace ModuleManager.SqlServer.Settings
{
    public class SmtpSettings
    {
        public string NameApp { get; set; } = null!;

        public string EMailAddress { get; set; } = null!;

        public bool UseSSL { get; set; }

        public string Host { get; set; } = null!;

        public int Port { get; set; }

        public bool UseStartTls { get; set; }

        public string UserName { get; set; } = null!;

        public string Password { get; set; } = null!;

    }
}
