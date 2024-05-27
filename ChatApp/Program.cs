using System;
using System.Threading.Tasks;
using ChatApp;


class Program
{
    private static string ipAddress;
    private static int port;
    public static Encryption encryption;

    //The main task that starts on Process Start
    private static async Task Main(string[] args)
    {
        AppDomain.CurrentDomain.ProcessExit += (sender, args) => NetworkManager.StopListener();
        await Welcome();
    }

    //Welcomes the user and prompts for port
    private static async Task Welcome()
    {
        ipAddress = await NetworkManager.GetIpAddress();
        Console.WriteLine($"Welcome to Chat App. Your IP is {ipAddress}. What port would you like to listen on? Recommended: 7777");
        var portSuccess = int.TryParse(Console.ReadLine(), out port);
        while (!portSuccess || NetworkManager.IsPortInUse(ipAddress, port))
        {
            Console.WriteLine("Error! That is either not a viable port or the port is in use. Please enter the port you want to listen on. Recommended: 7777");
            portSuccess = int.TryParse(Console.ReadLine(), out port);
        }

        Console.WriteLine($"Great! You are listening on port {port}. If a chat request is sent to this ip and port, you will receive it.");
        Console.WriteLine("If you want to connect to someone, say 'Connect', or you can say 'Listen' to wait for someone to connect to you. To exit, say 'Exit'.");
        await MainLoop();
    }

    //The main loop.  Cycles through possible inputs ask of the user
    private static async Task MainLoop()
    {
        await StartEncryption();
        
        while (true)
        {
            var input = Console.ReadLine()?.ToLower();

            if (input == "connect")
            {
                await Connect();
            }
            else if (input == "listen")
            {
                await Host();
            }
            else if (input == "exit")
            {
                NetworkManager.StopListener();
                break;
            }
        }
    }

    //Method called when the user wants to connect to a Host.  Prompts for the Host's information
    private static async Task Connect()
    {
        Console.WriteLine("What IP address do you want to connect to?");
        var hostIPAddr = Console.ReadLine();

        Console.WriteLine("What port do you want to connect to?");
        var portSuccess = int.TryParse(Console.ReadLine(), out var hostPort);

        while (!portSuccess)
        {
            Console.WriteLine("Error! Please retype port.");
            portSuccess = int.TryParse(Console.ReadLine(), out hostPort);
        }

        Console.WriteLine("Sending Connection Request...");

        await NetworkManager.Connect(hostIPAddr, hostPort);
    }

    //Method called when the user wants to be Host.  Starts a Listener
    private static async Task Host()
    {
        await NetworkManager.StartListener(ipAddress, port);
        Console.WriteLine("Waiting for a connection request...");
    }

    //Creates the encryption object for interaction
    private static async Task StartEncryption()
    {
        encryption = new Encryption();
    }
}
