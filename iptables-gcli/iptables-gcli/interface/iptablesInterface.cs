using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace iptables_gcli
{
    public class iptablesInterface
    {
        private List<dynamic> rules = new List<dynamic>();
        public int reorderRule(int fromIndex, int toIndex)
        {
            if (fromIndex < 0 || fromIndex >= rules.Count)
            {
                throw new ArgumentOutOfRangeException("fromIndex");
            }
            if (toIndex < 0 || toIndex >= rules.Count)
            {
                throw new ArgumentOutOfRangeException("toIndex");
            }
            if (fromIndex == toIndex)
            {
                return fromIndex;
            }
            Rule rule = rules[fromIndex];
            rules.RemoveAt(fromIndex);
            rules.Insert(toIndex, rule);
            return toIndex;
        }
        public int addRule(string rule)
        {
            if (ProgramState.Table == "nat")
            {
                rules.Add(new NATRule(rule));
            } else 
            {
                rules.Add(new Rule(rule));
            }
            return rules.Count - 1;
        }
        public int addRule(Rule rule)
        {
            rules.Add(rule);
            return rules.Count - 1;
        }
        public int addRule(NATRule rule)
        {
            rules.Add(rule);
            return rules.Count - 1;
        }

        public bool removeRule(int index)
        {
            if (rules.Count <= index)
            {
                return false;
            }
            rules.RemoveAt(index);
            return true;
        }
        public List<string> applyRules()
        {
            List<string> failedrules = new List<string>();
            #if !DEBUG
            string[] lines = new string[rules.Count];
            for (int i = 0; i < rules.Count; i++)
            {
                lines[i] = rules[i].fulllineFromValues;
            }
            //flush rules first
            {
                ProcessStartInfo psi = new ProcessStartInfo("iptables", $"-F -t {ProgramState.Table}");
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.CreateNoWindow = true;
                Process p = Process.Start(psi);
                p.WaitForExit();
            }
            {
                ProcessStartInfo psi = new ProcessStartInfo("iptables", $"-X -t {ProgramState.Table}");
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.CreateNoWindow = true;
                Process p = Process.Start(psi);
                p.WaitForExit();
            }
            foreach (string line in lines)
            {
                //run each line as iptables command
                ProcessStartInfo psi = new ProcessStartInfo("iptables", $"-t {ProgramState.Table} {line}");
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.CreateNoWindow = true;
                Process p = Process.Start(psi);
                p.WaitForExit();
                string output = p.StandardOutput.ReadToEnd();
                string error = p.StandardError.ReadToEnd();
                if (error != "")
                {
                    failedrules.Add(line + ": " + error);
                }
            }
            return failedrules;
            #else
            //simulate iptables command output
            return new List<string>() {  }; //no failed commands. todo: linting?
            #endif

        }
        public List<dynamic> getiptablesRules(bool refresh = false)
        {
            if (refresh)
            {
                rules.Clear();
#if !DEBUG // in case of debugging, we don't want to access the real iptables, so the application will simulate the output
                var proc = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/sbin/iptables",
                        Arguments = $"-S -t {ProgramState.Table}",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }//
                };
                proc.Start();
                string output = "";
                while (!proc.StandardOutput.EndOfStream)
                {
                    string line = proc.StandardOutput.ReadLine();
                    output += line + "\n" ;
                }
                proc.WaitForExit();
                if (proc.ExitCode != 0)
                {
                    throw new Exception($"Error while getting iptables rules: {output}");
                }
#else
                string output = "";
                if (ProgramState.Table == "filter")
                {
                    output = 
                    "-P INPUT ACCEPT\n" +
                    "-P FORWARD ACCEPT\n" +
                    "-P OUTPUT ACCEPT\n" +
                    "-N DOCKER\n" +
                    "-N DOCKER-ISOLATION-STAGE-1\n" +
                    "-N DOCKER-ISOLATION-STAGE-2\n" +
                    "-N DOCKER-USER\n" +
                    "-A INPUT -p udp -m udp --dport 1193 -j ACCEPT\n" +
                    "-A INPUT -p tcp -m tcp --dport 1194 -j ACCEPT\n" +
                    "-A FORWARD -j DOCKER-USER\n" +
                    "-A FORWARD -j DOCKER-ISOLATION-STAGE-1\n" +
                    "-A FORWARD -o docker0 -m conntrack --ctstate RELATED,ESTABLISHED -j ACCEPT\n" +
                    "-A FORWARD -o docker0 -j DOCKER\n" +
                    "-A FORWARD -i docker0 ! -o docker0 -j ACCEPT\n" +
                    "-A FORWARD -i docker0 -o docker0 -j ACCEPT\n" +
                    "-A FORWARD -o br-cd4f2c06e311 -m conntrack --ctstate RELATED,ESTABLISHED -j ACCEPT\n" +
                    "-A FORWARD -o br-cd4f2c06e311 -j DOCKER\n" +
                    "-A FORWARD -i br-cd4f2c06e311 ! -o br-cd4f2c06e311 -j ACCEPT\n" +
                    "-A FORWARD -i br-cd4f2c06e311 -o br-cd4f2c06e311 -j ACCEPT\n" +
                    "-A FORWARD -o br-146391d4b96d -m conntrack --ctstate RELATED,ESTABLISHED -j ACCEPT"; //simulated output in case of debugging
                } else if (ProgramState.Table == "nat")
                {
                    output =
                    "-P PREROUTING ACCEPT \n" +
                    "-P INPUT ACCEPT \n" +
                    "-P OUTPUT ACCEPT \n" +
                    "-P POSTROUTING ACCEPT\n" +
                    "-A PREROUTING -i enp9s0 -p tcp -m multiport --dports 80,443 -j DNAT --to-destination 10.0.0.3\n" +
                    "-A PREROUTING -s 10.0.0.0/24 -d 65.21.140.139/32 -p tcp -m multiport --dports 80,443 -j DNAT --to-destination 10.0.0.3\n" +
                    "-A PREROUTING -i enp9s0 -p tcp -m multiport --dports 32400 -j DNAT --to-destination 10.0.0.2\n" +
                    "-A POSTROUTING -o enp9s0 -j MASQUERADE \n" +
                    "-A DOCKER -d 127.0.0.1/32 ! -i br-146391d4b96d -p tcp -m tcp --dport 4000 -j DNAT --to-destination 172.24.0.2:4000\n" +
                    "-A POSTROUTING -s 10.0.0.0/24 -d 10.0.0.3/32 -p tcp -m multiport --dports 80,443 -j SNAT --to-source 10.0.0.1\n";
                }


#endif
                foreach (string line in output.Split('\n'))
                {
                    if (line.Trim() != "")
                    {
                        if (ProgramState.Table == "nat")
                        {
                            rules.Add(new NATRule(line));
                            
                        }
                        else
                        {
                            rules.Add(new Rule(line));
                        }
                    }
                }

            }

            return rules;
        }

        // Blank fields means "ignored"
        // IPv4 rules ONLY! (for now) -- new class will be created for IPv6 eventually
        public class Rule
        {
            public Rule() { }
            /// <summary>
            /// The regex used to parse the iptables output
            /// </summary>
            protected Regex commandLineParameterRegex = new Regex(@"(-[^ ]*?)( ([^-].*?)|)( |$|\n)");
            
            /// <summary>
            /// Create a rule object from iptables syntax
            /// </summary>
            public Rule(string line)
            {
                // Possible caveat: if a parameter contains a space, it will not match the regex. This should not happen, however there is a possibility that a value may be enclosed in quotation marks (edgecase).
                MatchCollection matches = commandLineParameterRegex.Matches(line); //matches commandlineparamters and their values
                List<string> detectedModules = new List<string>(); //only contains expicitly defined modules by `-m`. modules defined automatically are not included.
                foreach (Match part in matches)
                {
                    string commandLineOption = part.Groups[1].Value;
                    switch (commandLineOption)
                    {
                        case "-A":
                            ruleType = RuleType.ADD;
                            chain = part.Groups[3].Value;
                            break;
                        case "--dport":
                            destinationPort = part.Groups[3].Value;
                            break;
                        case "--sport":
                            sourcePort = part.Groups[3].Value;
                            break;
                        case "-j":
                            target = part.Groups[3].Value;
                            break;
                        case "-i":
                            inInterface = part.Groups[3].Value;
                            break;
                        case "-o":
                            outInterface = part.Groups[3].Value;
                            break;
                        case "-p":
                            protocol = part.Groups[3].Value;
                            break;
                        case "-s":
                            source = part.Groups[3].Value;
                            break;
                        case "-d":
                            destination = part.Groups[3].Value;
                            break;
                        case "--physdev-in":
                            physicalInInterface = part.Groups[3].Value;
                            detectedModules.Add("physdev");
                            break;
                        case "--physdev-out":
                            physicalOutInterface = part.Groups[3].Value;
                            detectedModules.Add("physdev");
                            break;
                        case "-f":
                            fragmentation = "yes";
                            break;
                        case "--tcp-flags":
                            tcpflags = part.Groups[3].Value;
                            break;
                        case "--tcp-option":
                            tcpOptionNumber = part.Groups[3].Value;
                            break;
                        case "--icmp-type":
                            icmpType = part.Groups[3].Value;
                            break;
                        case "--mac-source":
                            ethernetAddress = part.Groups[3].Value;
                            if (!detectedModules.Contains("mac"))
                            {
                                detectedModules.Add("mac");
                            }
                            break;
                        case "--limit":
                            packetflowRate = part.Groups[3].Value;
                            if (!detectedModules.Contains("limit"))
                            {
                                detectedModules.Add("limit");
                            }
                            break;
                        case "--limit-burst":
                            packetburstRate = part.Groups[3].Value;
                            if (!detectedModules.Contains("limit"))
                            {
                                detectedModules.Add("limit");
                            }
                            break;
                        case "--ctstate":
                            connectionStates = part.Groups[3].Value;
                            if (!detectedModules.Contains("ctstate"))
                            {
                                //detectedModules.Add("ctstate");
                            }
                            break;
                        case "--uid-owner": //not sure about this one right now.
                            service = part.Groups[3].Value;
                            if (!detectedModules.Contains("owner"))
                            {
                                detectedModules.Add("owner");
                            }
                            break;
                        case "--physdev-is-in":
                            packetIncomingBridgeInterface = part.Groups[3].Value;
                            if (!detectedModules.Contains("physdev"))
                            {
                                detectedModules.Add("physdev");
                            }
                            break;
                        case "--physdev-is-out":
                            packetOutgoingBridgeInterface = part.Groups[3].Value;
                            if (!detectedModules.Contains("physdev"))
                            {
                                detectedModules.Add("physdev");
                            }
                            break;
                        case "--dports":
                            destinationPort = part.Groups[3].Value;
                            if (!detectedModules.Contains("multiport"))
                            {
                                detectedModules.Add("multiport");
                            }
                            break;
                        case "--sports":
                            sourcePort = part.Groups[3].Value;
                            if (!detectedModules.Contains("multiport"))
                            {
                                detectedModules.Add("multiport");
                            }
                            break;
                        case "-m":
                            // add module to list of detected modules
                            if (!detectedModules.Contains(part.Groups[3].Value))
                            {
                                detectedModules.Add(part.Groups[3].Value);
                            }
                            break;
                        default:
                            additionalParameters += part.Value; //
                            //IsManualRule = true; //an unsupported commandlineoption was set for this rule, so it is a manual rule.
                            break;
                    }
                }
                additionalParameters = additionalParameters.Trim();
                // check whether the module is part of a supported commandlineoption. if not, add to additionalmodules. SEE COMMENT AT fulllineFromValues!!
                List<string> nonAdditionalModules = new List<string>() { "physdev", "limit", "owner", "tcp", "udp" }; //list of modules that are not added as additional parameters.
                if (detectedModules.Count > 0)
                {
                    foreach (string module in detectedModules)
                    {
                        if (!nonAdditionalModules.Contains(module))
                        {
                            additionalModules += module + ",";
                        }
                    }
                    additionalModules = additionalModules.Trim(','); //remove trailing comma from finished string
                }
                string notMatched = commandLineParameterRegex.Replace(line, "");
                // Some rules do not set the -j commandlineoption, so it will not match the regex. If the non-matching string is not an expected target it will be ignored.
                // This is a hack considering that it is not possible to know for sure whether the nonmatching string is a target or not if it still matches the expected value of a target (i.e. it is meant as a different parameter). This should not happen assuming iptables output is consistent.
                switch (notMatched.Trim())
                {
                    case "ACCEPT":
                        target = "ACCEPT";
                        break;
                    case "DROP":
                        target = "DROP";
                        break;
                    case "REJECT":
                        target = "REJECT";
                        break;
                    case "RETURN":
                        target = "RETURN";
                        break;
                    case "LOG":
                        target = "LOG";
                        break;
                    case "MASQUERADE":
                        target = "MASQUERADE";
                        break;
                    default:
                        break;
                }
                unprocessedFullLine=line;
            }
            // This is more of a reference than anything else right now.
            public struct Target
            {
                public const string ACCEPT = "ACCEPT";
                public const string DROP = "DROP";
                public const string REJECT = "REJECT";
                public const string LOG = "LOG";
                public const string MASQUERADE = "MASQUERADE";
                public const string NOTRACK = "NOTRACK";
                public const string REDIRECT = "REDIRECT";
            }
            // Types other than "ADD" are not supported yet.
            public struct RuleType
            {
                public const string ADD = "-A";
                public const string DELETE = "-D";
                public const string NEWCHAIN = "-N";
                public const string POLICY = "-P";
                public const string INSERT = "-I";
                public const string REPLACE = "-R";
                public const string FLUSH = "-F";
                public const string DEFAULT = "-D";

            }
            //public bool IsManualRule { get; set; } = false; //Not needed, since additional parameters may be added to the additionalParameters string.
            private string _ruleType;
            public string ruleType { get { return _ruleType; } set { _ruleType = value; } } // do something with this?
            private string _chain = "";
            public string chain { get { if (_chain == "") { return "UNKNOWN"; }; return _chain; } set { _chain = value; } }
            public string target { get; set; } = "";
            //public string number { get { return AddRuleRegex.Match(workingsetFullLine).Groups[2].Value; } set { } }
            //public string size { get; set; } = "";
            public string protocol { get; set; } = "";
            public string source { get; set; } = "";
            public string destination { get; set; } = "";

            public string destinationPort { get; set; } = "";
            public string sourcePort { get; set; } = "";
            public string outInterface { get; set; } = "";
            public string inInterface { get; set; } = "";
            //public string comment { get; set; } = "";
            // generates a new fullline from the current rule. falls back to the original fullline if the rule is not valid
            public virtual string fulllineFromValues
            {
                get
                {
                    // TODO: decide whether to set modules for each module parameter or set them collectively at the end. does adding the same module multiple times even work? If not, then we need to set the modules only once by defining a list of detected modules.
                    if (ruleType != Rule.RuleType.ADD /*|| IsManualRule*/) { return $"{unprocessedFullLine}"; } // if the rule is not explicitly an "ADD" rule, then stop attempting to craft a fullline from the values.
                    string returnString = "";
                    returnString += $"{ruleType} {chain}";
                    if (additionalModules != "") { returnString += $" -m {String.Join(" -m ", additionalModules.Split(","))}"; } //basic implementation at best.
                    if (protocol != "") { returnString += $" -p {protocol}"; }
                    if (source != "") { returnString += $" -s {source}"; }
                    if (destination != "") { returnString += $" -d {destination}"; }
                    if (destinationPort != "") 
                    { 
                        if (additionalModules.Contains("multiport"))
                        {
                            returnString += $" --dports {destinationPort}"; 
                        } else 
                        {
                            returnString += $" --dport {destinationPort}"; 
                        }
                    }
                    if (sourcePort != "") 
                    { 
                        if (additionalModules.Contains("multiport"))
                        {
                            returnString += $" --sports {sourcePort}"; 
                        } else 
                        {
                            returnString += $" --dport {sourcePort}"; 
                        }
                    }
                    if (outInterface != "") { returnString += $" -o {outInterface}"; }
                    if (inInterface != "") { returnString += $" -i {inInterface}"; }
                    if (physicalInInterface != "") { returnString += $" -m physdev --physdev-in {physicalInInterface}"; }
                    if (physicalOutInterface != "") { returnString += $" -m physdev --physdev-out {physicalOutInterface}"; }
                    if (fragmentation == "yes") { returnString += $" -f"; }
                    if (tcpflags != "") { returnString += $" --tcp-flags {tcpflags}"; }
                    if (tcpOptionNumber != "") { returnString += $" --tcp-option {tcpOptionNumber}"; }
                    if (icmpType != "") { returnString += $" --icmp-type {icmpType}"; }
                    if (ethernetAddress != "") { returnString += $" -m mac --mac-source {ethernetAddress}"; }
                    if (packetflowRate != "") { returnString += $" -m limit --limit {packetflowRate}"; }
                    if (packetburstRate != "") { returnString += $" -m limit --limit-burst {packetburstRate}"; }
                    if (connectionStates != "") { returnString += $" --ctstate {connectionStates}"; }
                    if (service != "") { returnString += $" -m owner --uid-owner {service}"; } //not sure about this one right now.
                    if (packetIncomingBridgeInterface != "") { returnString += $" -m physdev --physdev-is-in {packetIncomingBridgeInterface}"; }
                    if (packetOutgoingBridgeInterface != "") { returnString += $" -m physdev --physdev-is-out {packetOutgoingBridgeInterface}"; }
                    if (packetBeingBridged != "") { returnString += $" -m physdev --physdev-is-bridged {packetBeingBridged}"; }
                    if (additionalParameters != "") { returnString += $" {additionalParameters}"; }
                    if (target != "") { returnString += $" -j {target}"; }

                    return returnString;
                }
            }
            public string physicalInInterface {get;set ;} = "";
            public string physicalOutInterface {get;set ;} = "";
            public string fragmentation {get;set ;} = "";
            public string tcpflags {get;set ;} = "";
            public string tcpOptionNumber {get;set ;} = "";
            public string icmpType {get;set ;} = "";
            public string ethernetAddress {get;set ;} = "";
            public string packetflowRate {get;set ;} = "";
            public string packetburstRate {get;set ;} = "";
            public string connectionStates {get;set ;} = "";
            public string service {get;set ;} = "";
            public string packetIncomingBridgeInterface {get;set ;} = "";
            public string packetOutgoingBridgeInterface {get;set ;} = "";
            public string packetBeingBridged {get;set ;} = "";
            public string additionalModules {get;set ;} = "";
            public string additionalParameters {get;set ;} = "";
            protected string unprocessedFullLine { get; set; } = ""; //the rule string that the rule was created from. used for manual rules.
        }

        /// <summary>
        /// A rule that has NAT-table functionality
        /// </summary>
        public class NATRule : Rule
        {
            public NATRule() : base() { }
            public NATRule(string fullline) : base(fullline)
            {
                MatchCollection matches = commandLineParameterRegex.Matches(additionalParameters);
                foreach (Match part in matches)
                {
                    string commandLineOption = part.Groups[1].Value;
                    switch (commandLineOption)
                    {
                        case "--to-destination":
                            toDestination = part.Groups[3].Value;
                            additionalParameters = additionalParameters.Replace(part.Value, "");
                            break;
                        case "--to-source":
                            toSource = part.Groups[3].Value;
                            additionalParameters = additionalParameters.Replace(part.Value, "");
                            break;
                        default:
                            // ignore unrelated options
                            break;
                    }
                }
            }
            public override string fulllineFromValues
            { 
                get
                {
                    if (ruleType != Rule.RuleType.ADD) { return $"{unprocessedFullLine}"; } // if the rule is not explicitly an "ADD" rule, then stop attempting to craft a fullline from the values.
                    string returnString = "";
                    if (toDestination != "") { returnString += $" --to-destination {toDestination}"; }
                    if (toSource != "") { returnString += $" --to-source {toSource}"; }
                    return $"{base.fulllineFromValues}{returnString}"; //TODO: Find a way to properly position NAT arguments
                }
            }
            public string toDestination { get; set; } = "";
            public string toSource { get; set; } = "";
        }
    }
}