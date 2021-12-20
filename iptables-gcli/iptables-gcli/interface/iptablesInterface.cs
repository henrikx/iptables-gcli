using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace iptables_gcli
{
    public class iptablesInterface
    {
        private List<Rule> rules = new List<Rule>();
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
            rules.Add(new Rule(rule));
            return rules.Count - 1;
        }
        public int addRule(Rule rule)
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
        // private List<string> runIPTablesCommand(string arguments)
        // {

        // }
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
                ProcessStartInfo psi = new ProcessStartInfo("iptables", "-F");
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.CreateNoWindow = true;
                Process p = Process.Start(psi);
                p.WaitForExit();
            }
            {
                ProcessStartInfo psi = new ProcessStartInfo("iptables", "-X");
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
                ProcessStartInfo psi = new ProcessStartInfo("iptables", line);
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
            return new List<string>() { "gg" }; //no failed commands. todo: linting?
            #endif

        }
        public List<Rule> getiptablesRules(bool refresh = false)
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
                        Arguments = "-S",
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
#else
                string output = "-P INPUT ACCEPT\n" +
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

#endif
                foreach (string line in output.Split('\n'))
                {
                    if (line.Trim() != "")
                    {
                        rules.Add(new Rule(line));
                    }
                }

            }

            return rules;
        }

        // Blank fields means "ignored"
        // IPv4 rules ONLY! (for now) -- new class will be created for IPv6 eventually
        public class Rule
        {
            /// <summary>
            /// Create a rule from parameters
            /// </summary>
            public Rule (string RuleType, string Chain, string Protocol, string Source, string Destination, string Target, string OutInterface, string InInterface, string DestinationPort, string SourcePort, string PhysicalInInterface, string PhysicalOutInterface, string Fragmentation, string TcpFlags, string TcpOptionNumber, string IcmpType, string EthernetAddress, string PacketFlowRate, string PacketBurstRate, string ConnectionStates, string Service, string PacketIncomingBridgeInterface, string PacketOutgoingBridgeInterface, string PacketBeingBridged, string AdditionalModules, string AdditionalParameters, string OriginalFullLine = "")
            {
                ruleType = RuleType;
                chain = Chain;
                protocol = Protocol;
                source = Source;
                destination = Destination;
                destinationPort = DestinationPort;
                sourcePort = SourcePort;
                target = Target;
                outInterface = OutInterface;
                inInterface = InInterface;
                physicalInInterface = PhysicalInInterface;
                physicalOutInterface = PhysicalOutInterface;
                fragmentation = Fragmentation;
                tcpflags = TcpFlags;
                tcpOptionNumber = TcpOptionNumber;
                icmpType = IcmpType;
                ethernetAddress = EthernetAddress;
                packetflowRate = PacketFlowRate;
                packetburstRate = PacketBurstRate;
                connectionStates = ConnectionStates;
                service = Service;
                packetIncomingBridgeInterface = PacketIncomingBridgeInterface;
                packetOutgoingBridgeInterface = PacketOutgoingBridgeInterface;
                additionalModules = AdditionalModules;
                additionalParameters = AdditionalParameters;
                packetBeingBridged = PacketBeingBridged;

                if (OriginalFullLine != "")
                {
                    unprocessedFullLine = OriginalFullLine;
                }
                else
                {
                    unprocessedFullLine = fulllineFromValues;
                }
            }
            /// <summary>
            /// Create a rule object from iptables syntax
            /// </summary>
            public Rule(string line)
            {
                // Possible caveat: if a parameter contains a space, it will not match the regex. This should not happen, however there is a possibility that a value may be enclosed in quotation marks (edgecase).
                // Possible caveat 2: modules defined by "-m" are not captured, however they are correctly generated based on the output if a supported module is used.
                // TODO: there needs to be a way to define when a rule is output as a manual rule, and when it is a generated rule.
                Regex commandLineParameterRegex = new Regex(@"(-.*?) (.*?)( |$|\n)");
                MatchCollection matches = commandLineParameterRegex.Matches(line); //matches commandlineparamters and their values
                List<string> detectedModules = new List<string>(); //only contains expicitly defined modules by `-m`. modules defined automatically are not included.
                foreach (Match part in matches)
                {
                    string commandLineOption = part.Groups[1].Value;
                    switch (commandLineOption)
                    {
                        case "-A":
                            ruleType = RuleType.ADD;
                            chain = part.Groups[2].Value;
                            break;
                        case "--dport":
                            destinationPort = part.Groups[2].Value;
                            break;
                        case "--sport":
                            sourcePort = part.Groups[2].Value;
                            break;
                        case "-j":
                            target = part.Groups[2].Value;
                            break;
                        case "-i":
                            inInterface = part.Groups[2].Value;
                            break;
                        case "-o":
                            outInterface = part.Groups[2].Value;
                            break;
                        case "-p":
                            protocol = part.Groups[2].Value;
                            break;
                        case "-s":
                            source = part.Groups[2].Value;
                            break;
                        case "-d":
                            destination = part.Groups[2].Value;
                            break;
                        case "--physdev-in":
                            physicalInInterface = part.Groups[2].Value;
                            detectedModules.Add("physdev");
                            break;
                        case "--physdev-out":
                            physicalOutInterface = part.Groups[2].Value;
                            detectedModules.Add("physdev");
                            break;
                        case "-f":
                            fragmentation = part.Groups[2].Value;
                            break;
                        case "--tcp-flags":
                            tcpflags = part.Groups[2].Value;
                            break;
                        case "--tcp-option":
                            tcpOptionNumber = part.Groups[2].Value;
                            break;
                        case "--icmp-type":
                            icmpType = part.Groups[2].Value;
                            break;
                        case "--mac-source":
                            ethernetAddress = part.Groups[2].Value;
                            if (!detectedModules.Contains("mac"))
                            {
                                detectedModules.Add("mac");
                            }
                            break;
                        case "--limit":
                            packetflowRate = part.Groups[2].Value;
                            if (!detectedModules.Contains("limit"))
                            {
                                detectedModules.Add("limit");
                            }
                            break;
                        case "--limit-burst":
                            packetburstRate = part.Groups[2].Value;
                            if (!detectedModules.Contains("limit"))
                            {
                                detectedModules.Add("limit");
                            }
                            break;
                        case "--ctstate":
                            connectionStates = part.Groups[2].Value;
                            if (!detectedModules.Contains("ctstate"))
                            {
                                //detectedModules.Add("ctstate");
                            }
                            break;
                        case "--uid-owner": //not sure about this one right now.
                            service = part.Groups[2].Value;
                            if (!detectedModules.Contains("owner"))
                            {
                                detectedModules.Add("owner");
                            }
                            break;
                        case "--physdev-is-in":
                            packetIncomingBridgeInterface = part.Groups[2].Value;
                            if (!detectedModules.Contains("physdev"))
                            {
                                detectedModules.Add("physdev");
                            }
                            break;
                        case "--physdev-is-out":
                            packetOutgoingBridgeInterface = part.Groups[2].Value;
                            if (!detectedModules.Contains("physdev"))
                            {
                                detectedModules.Add("physdev");
                            }
                            break;
                        case "-m":
                            // add module to list of detected modules
                            if (!detectedModules.Contains(part.Groups[2].Value))
                            {
                                detectedModules.Add(part.Groups[2].Value);
                            }
                            break;
                        default:
                            additionalParameters += commandLineOption + " " + part.Groups[2].Value + " "; //
                            //IsManualRule = true; //an unsupported commandlineoption was set for this rule, so it is a manual rule.
                            break;
                    }
                }
                // check whether the module is part of a supported commandlineoption. if not, add to additionalmodules. SEE COMMENT AT fulllineFromValues!!
                List<string> nonAdditionalModules = new List<string>() { "physdev", "limit", "state", "owner", "tcp", "udp" }; //list of modules that are not added as additional parameters.
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


                // Regex AddRuleRegex = new Regex(@"(-(A|P|N|I|R|F|D)) (.*?) ");
                // Regex findCommandActionRegex = new Regex(@"(-.)[ |\n]"); //group 1
                // Regex protocolRegex = new Regex(@"-p (tcp|udp)"); //group 1
                // Regex sourceRegex = new Regex(@"-s (\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\/\d{1,2})"); //group 1
                // Regex destinationRegex = new Regex(@"-d (\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\/\d{1,2})"); //group 1
                // Regex outInterfaceRegex = new Regex(@"-o ((\w|-)+)"); //group 1
                // Regex inInterfaceRegex = new Regex(@"-i ((\w|-)+)"); //group 1
                // Regex targetRegex = new Regex(@"-j (\w+)"); //group 1
                // Regex commentRegex = new Regex(@"# (.*)"); //group 1
                // Regex sizeRegex = new Regex(@"-m (\w+)"); //group 1
                // Regex destinationPortRegex = new Regex(@"--dport (\d{1,5})");
                // Regex sourcePortRegex = new Regex(@"--sport (\d{1,5})");


                // originalFullLine = line;
                // chain = AddRuleRegex.Match(line).Groups[3].Value;
                // target = targetRegex.Match(line).Groups[1].Value;
                // source = sourceRegex.Match(line).Groups[1].Value;
                // destination = destinationRegex.Match(line).Groups[1].Value;
                // destinationPort = destinationPortRegex.Match(line).Groups[1].Value;
                // sourcePort = sourcePortRegex.Match(line).Groups[1].Value;
                // outInterface = outInterfaceRegex.Match(line).Groups[1].Value;
                // inInterface = inInterfaceRegex.Match(line).Groups[1].Value;
                // protocol = protocolRegex.Match(line).Groups[1].Value;

            }
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
            //private string _source = "";
            //public string source { get {if (_source == "") { return "0.0.0.0/0"; } return _source; } set {_source = value; } }
            //private string _destination = "";
            //public string destination { get {if (_destination == "") { return "0.0.0.0/0"; } return _destination; } set { _destination = value; } }
            //public string destinationParameters {get { return "-d " + destination; } }

            public string destinationPort { get; set; } = "";
            public string sourcePort { get; set; } = "";
            public string outInterface { get; set; } = "";
            public string inInterface { get; set; } = "";
            //public string comment { get; set; } = "";
            // generates a new fullline from the current rule. falls back to the original fullline if the rule is not valid
            public string fulllineFromValues
            {
                get
                {
                    // TODO: decide whether to set modules for each module parameter or set them collectively at the end. does adding the same module multiple times even work? If not, then we need to set the modules only once by defining a list of detected modules.
                    if (ruleType != Rule.RuleType.ADD /*|| IsManualRule*/) { return $"{unprocessedFullLine}"; } // if the rule is not explicitly an "ADD" rule, then stop attempting to craft a fullline from the values.
                    string returnString = "";
                    returnString += $"{ruleType} {chain}";
                    if (protocol != "") { returnString += $" -p {protocol}"; }
                    if (source != "") { returnString += $" -s {source}"; }
                    if (destination != "") { returnString += $" -d {destination}"; }
                    if (destinationPort != "") { returnString += $" --dport {destinationPort}"; }
                    if (sourcePort != "") { returnString += $" --sport {sourcePort}"; }
                    if (outInterface != "") { returnString += $" -o {outInterface}"; }
                    if (inInterface != "") { returnString += $" -i {inInterface}"; }
                    if (physicalInInterface != "") { returnString += $" -m physdev --physdev-in {physicalInInterface}"; }
                    if (physicalOutInterface != "") { returnString += $" -m physdev --physdev-out {physicalOutInterface}"; }
                    if (fragmentation == "yes") { returnString += $" -f"; }
                    if (tcpflags != "") { returnString += $" --tcp-flags {tcpflags}"; }
                    if (tcpOptionNumber != "") { returnString += $" --tcp-option {tcpOptionNumber}"; }
                    if (icmpType != "") { returnString += $" --icmp-type {icmpType}"; }
                    if (ethernetAddress != "") { returnString += $" -m mac --mac-source {ethernetAddress}"; }
                    if (packetflowRate != "") { returnString += $" -m limit --limit {packetflowRate}/s"; }
                    if (packetburstRate != "") { returnString += $" -m limit --limit-burst {packetburstRate}"; }
                    if (connectionStates != "") { returnString += $" --ctstate {connectionStates}"; }
                    if (service != "") { returnString += $" -m owner --uid-owner {service}"; } //not sure about this one right now.
                    if (packetIncomingBridgeInterface != "") { returnString += $" -m physdev --physdev-is-in {packetIncomingBridgeInterface}"; }
                    if (packetOutgoingBridgeInterface != "") { returnString += $" -m physdev --physdev-is-out {packetOutgoingBridgeInterface}"; }
                    if (packetBeingBridged != "") { returnString += $" -m physdev --physdev-is-bridged {packetBeingBridged}"; }
                    if (additionalModules != "") { returnString += $" -m {String.Join(" -m ", additionalModules.Split(","))}"; } //basic implementation at best.
                    if (additionalParameters != "") { returnString += $" {additionalParameters}"; }
                    if (target != "") { returnString += $" -j {target}"; }

                    return returnString;

                    //return $"{ruleType} {chain}{(protocol != "" ? $" -p {protocol}" : "")}{(source != "" && source != "0.0.0.0/0" ? $" -s {source} " : "")}{(destination != "" && destination != "0.0.0.0/0" ? $" -d {destination} " : "")}{(destinationPort != "" ? $" --dport {destinationPort}" : "")}{(sourcePort != "" ? $" --sport {sourcePort}" : "")}{(outInterface != "" ? $" -o {outInterface}" : "")}{(inInterface != "" ? $" -i {inInterface}" : "")}{(target != "" ? $" -j {target}" : "")}";
                    //return $"-A {chain}{(protocol != "" ? $" -p {protocol}" : "")} {(source != "" && source != "0.0.0.0/0" ? $"-s {source} " : "")}{(destination != "" && destination != "0.0.0.0/0" ? $"-d {destination} " : "")}{(destinationPort != "" ? $"--dport {destinationPort} " : "")}{(sourcePort != "" ? $"--sport {sourcePort} " : "")}-j {target}";

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
            private string unprocessedFullLine { get; set; } = ""; //the rule string that the rule was created from. used for manual rules.
        }
    }
}