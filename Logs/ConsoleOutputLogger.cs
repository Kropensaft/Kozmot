namespace OpenGL;

public class Logger
{
    private static readonly string LogDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

    private static Logger _LoggerSingleton;

    public Logger()
    {
        EnsureLogDirectoryExists();
        InstantiateStreamWriter();
    }

    private static Logger LoggerSingleton
    {
        get
        {
            if (_LoggerSingleton == null) _LoggerSingleton = new Logger();
            return _LoggerSingleton;
        }
    }

    public StreamWriter SW { get; set; }

    ~Logger()
    {
        if (SW != null)
            try
            {
                SW.Dispose();
            }
            catch (ObjectDisposedException)
            {
            } // object already disposed - ignore exception
    }

    public static void WriteLine(string str)
    {
        Console.WriteLine(str);
        LoggerSingleton.SW.WriteLine(str);
    }

    public static void Write(string str)
    {
        Console.Write(str);
        LoggerSingleton.SW.Write(str);
    }

    private void InstantiateStreamWriter()
    {
        string filePath = Path.Combine(LogDirPath, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")) + ".txt";
        try
        {
            SW = new StreamWriter(filePath);
            SW.AutoFlush = true;
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new ApplicationException(
                string.Format("Access denied. Could not instantiate StreamWriter using path: {0}.", filePath), ex);
        }
    }

    private void EnsureLogDirectoryExists()
    {
        if (!Directory.Exists(LogDirPath))
            try
            {
                Directory.CreateDirectory(LogDirPath);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new ApplicationException(
                    string.Format("Access denied. Could not create log directory using path: {0}.", LogDirPath), ex);
            }
    }
}