using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace wa_api_incomm.Help
{
    public class EncoderAES
    {
        private string path;
        private string key;
        public EncoderAES(string path, string key)
        {
            this.key = key;
            this.path = path;
        }

        public string Encrypt(string text) => getTextFromJar("ENCRYPT", text);
        public string Decrypt(string text) => getTextFromJar("DECRYPT", text);


        public string getTextFromJar(string action, string text)
        {

            Process p = new Process();
            p.StartInfo = new ProcessStartInfo("java", $"-jar {path} {action} {key} {text}");
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;
            p.Start();
            string s = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            return Regex.Replace(s, @"\t|\n|\r", "");
        }
    }
}
