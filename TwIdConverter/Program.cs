using CoreTweet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TwIdConverter {
    class Program {
        private Tokens tokens;

        static void Main(string[] args) {
            (new Program()).exec(args);
        }

        private void exec(string[] args) {
            var opts = Option(args);
            foreach (var opt in opts) {
                Console.WriteLine($"{opt.Key}: {opt.Value}");
            }
            Console.WriteLine();

            if (opts.Any(value => value.Value == null)) {
                Console.WriteLine("Usage:");
                Console.WriteLine("    -key api_key");
                Console.WriteLine("    -key_secret api_secret");
                Console.WriteLine("    -token token");
                Console.WriteLine("    -token_secret token_secret");
                return;
            }
            tokens = Tokens.Create(opts["key"], opts["key_secret"], opts["token"], opts["token_secret"]);

            var regexs = new KeyValuePair<Regex, Action<Match>>[] {
                new KeyValuePair<Regex, Action<Match>>(
                    new Regex(@"^https?://twitter\.com/(?<user>[a-zA-Z0-9_]{1,15})/status/(?<id>.+)$"),
                    m => {
                        var id = getId(m.Groups["user"].Value);
                        var tweet = m.Groups["id"].Value;
                        Console.WriteLine($"https://twitter.com/{id}/status/{tweet}");
                    }
                ),
                new KeyValuePair<Regex, Action<Match>>(
                    new Regex(@"^https?://twitter\.com/@?(?<user>[a-zA-Z0-9_]{1,15})$"),
                    m => {
                        var id = getId(m.Groups["user"].Value);
                        Console.WriteLine($"https://twitter.com/{id}");
                    }
                ),
                new KeyValuePair<Regex, Action<Match>>(
                    new Regex(@"^(?<user>[a-zA-Z0-9_]{1,15})$"),
                    m => {
                        var id = getId(m.Groups["user"].Value);
                        Console.WriteLine(id);
                    }
                ),
            };

            string tmp;
            Func<string> read = () => {
                Console.Write("> ");
                return Console.ReadLine();
            };

            while ((tmp = read()) != null) {
                if (tmp == "exit") {
                    break;
                }

                bool f = true;
                foreach (var item in regexs) {
                    var match = item.Key.Match(tmp);
                    if (match.Success) {
                        item.Value(match);
                        f = false;
                        break;
                    }
                }

                if (f) {
                    Console.Error.WriteLine("Parse failed");
                }
            }
        }

        private string getId(string screenName) {
            var user = tokens.Users.Show(screenName);
            return user.Id.ToString();
        }

        private Dictionary<string, string> Option(string[] args) {
            // https://qiita.com/Marimoiro/items/a090344432a5f69e1fac
            args = args.Concat(new string[] { "" }).ToArray();
            var op = new string[] { "-key", "-key_secret", "-token", "-token_secret" };
            return op.ToDictionary(p => p.Substring(1), p => args.SkipWhile(a => a != p).Skip(1).FirstOrDefault());
        }
    }
}
