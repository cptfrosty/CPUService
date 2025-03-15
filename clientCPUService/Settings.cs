namespace clientCPUService
{
    internal class Settings
    {
        public string Path { get; set; }
        public int Hour { get; set; }
        public int Minutes { get; set; }
        public int MaximumProcessorLoad { get; set; }

        public int GetSeconds()
        {
            return (Hour * 3600) + (Minutes * 60);
        }
    }
}
