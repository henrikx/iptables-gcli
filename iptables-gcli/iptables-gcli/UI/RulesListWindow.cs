using System;
using Terminal.Gui;
using System.Linq;
using System.Collections.Generic;

namespace iptables_gcli
{
    public class RulesListWindow : Window
    {
        //TODO: Dialogboxes to separate classes? Or at least a separate file. This file is too long.
        public RulesListWindow(string title) : base(title)
        {
           // ColorScheme.Normal = new Terminal.Gui.Attribute(Color.Black, Color.White);
           // ColorScheme.HotNormal = new Terminal.Gui.Attribute(Color.White, Color.Black);
           // ColorScheme.HotFocus = new Terminal.Gui.Attribute(Color.Gray, Color.Black);
           // ColorScheme.Focus = new Terminal.Gui.Attribute(Color.Black, Color.White);
            var quit = new Button("Quit")
            {
                X = 0,
                Y = Pos.Bottom(this) - 4
            };
            quit.Clicked += () => Application.RequestStop();
            var edit = new Button("Edit")
            {
                X = Pos.Right(quit) + 1,
                Y = Pos.Bottom(this) - 4,
                IsDefault = true
            };
            var manualedit = new Button("Manual Edit")
            {
                X = Pos.Right(edit) + 1,
                Y = Pos.Bottom(this) - 4
            };
            var add = new Button("Add")
            {
                X = Pos.Right(manualedit) + 1,
                Y = Pos.Bottom(this) - 4
            };
            var del = new Button("Delete")
            {
                X = Pos.Right(add) + 1,
                Y = Pos.Bottom(this) - 4
            };


            var save = new Button("Apply To iptables")
            {
                X = Pos.Right(del) + 1,
                Y = Pos.Bottom(this) - 4
            };

            iptablesInterface ipt = new iptablesInterface();
            //string[] rules = ipt.getiptablesRules().ToArray();
            var rulesListview = new ListView(/*rules*/)
            {
                Width = Dim.Fill(),
                Height = Dim.Fill() - 4
            };

            rulesListview.SetSource(ipt.getiptablesRules(true).Select(x => x.fulllineFromValues).ToList());
            rulesListview.SelectedItem = 0;

            var moveRuleUp = new Button("Move Up")
            {
                X = Pos.Right(save) + 1,
                Y = Pos.Bottom(this) - 4
            };
            var moveRuleDown = new Button("Move Down")
            {
                X = Pos.Right(moveRuleUp) + 1,
                Y = Pos.Bottom(this) - 4
            };
            moveRuleUp.Clicked += () =>
            {
                if (rulesListview.SelectedItem > 0)
                {
                    int selectedItem = rulesListview.SelectedItem;
                    ipt.reorderRule(rulesListview.SelectedItem, rulesListview.SelectedItem - 1);
                    rulesListview.SetSource(ipt.getiptablesRules().Select(x => x.fulllineFromValues).ToList());
                    rulesListview.SelectedItem = selectedItem - 1;
                }
            };
            moveRuleDown.Clicked += () =>
            {
                if (rulesListview.SelectedItem < ipt.getiptablesRules().Count() - 1)
                {
                    int selectedItem = rulesListview.SelectedItem;
                    ipt.reorderRule(rulesListview.SelectedItem, rulesListview.SelectedItem + 1);
                    rulesListview.SetSource(ipt.getiptablesRules().Select(x => x.fulllineFromValues).ToList());
                    rulesListview.SelectedItem = selectedItem + 1;
                }
            };
            Action editmanualRuleDialog = () =>
            {
                int selectedRuleIndex = rulesListview.SelectedItem;
                iptablesInterface.Rule selectedRule = ipt.getiptablesRules()[selectedRuleIndex];
                var dialog = new Dialog("Edit Manual Rule")
                {
                    X = Pos.Center(),
                    Y = Pos.Center(),
                    Height = Dim.Fill() - 6
                };
                //dialog.ColorScheme = new ColorScheme()
                //{
                //    Normal = new Terminal.Gui.Attribute(Color.White, Color.Black),
                //    Focus = new Terminal.Gui.Attribute(Color.Black, Color.White),
                //    HotFocus = new Terminal.Gui.Attribute(Color.White, Color.Black),
                //    HotNormal = new Terminal.Gui.Attribute(Color.White, Color.Black)
                //};
                var exitDialog = new Button("Exit")
                {
                    X = Pos.Center() - 13,
                    Y = Pos.Bottom(dialog) - 7
                };
                exitDialog.Clicked += () =>
                {
                    //dialog.Running = false;
                    Application.RequestStop();

                };
                var saveAndExitDialog = new Button("Save and Exit")
                {
                    X = Pos.Center() + 3,
                    Y = Pos.Bottom(dialog) - 7
                };
                var textboxLabel = new Label("Rule line:")
                {
                    X = 0,
                    Y = 1
                };
                var textbox = new TextField(selectedRule.fulllineFromValues)
                {
                    X = 20,
                    Y = 1,
                    Width = Dim.Fill(1)
                };
                saveAndExitDialog.Clicked += () =>
                {
                    if (ProgramState.Table == "nat")
                    {
                        ipt.getiptablesRules()[rulesListview.SelectedItem] = new iptablesInterface.NATRule(textbox.Text.ToString());
                    }
                    else
                    {
                        ipt.getiptablesRules()[rulesListview.SelectedItem] = new iptablesInterface.Rule(textbox.Text.ToString());
                    }
                    //dialog.Running = false;
                    Application.RequestStop();
                    rulesListview.SetSource(ipt.getiptablesRules().Select(x => x.fulllineFromValues).ToList());
                    rulesListview.SelectedItem = selectedRuleIndex;
                };
                dialog.Add(textboxLabel);
                dialog.Add(textbox);
                dialog.Add(textboxLabel);
                dialog.Add(textbox);
                dialog.Add(exitDialog);
                dialog.Add(saveAndExitDialog);

                Application.Run(dialog);

            };
            #region PortRule Edit
            Action editPortRuleDialog = () =>
            {
                Dialog dialog = new Dialog("Edit Rule")
                {
                    X = Pos.Center(),
                    Y = Pos.Center(),
                    Height = Dim.Fill(3),
                    Width = Dim.Fill(5)
                };
                ScrollView scrollview = new ScrollView()
                {
                    X = 2,
                    Y = 2,
                    Width = Dim.Fill(1),
                    Height = Dim.Fill(1),
                    ShowVerticalScrollIndicator = true,
                    ContentSize = new Size(100, 100),
                    AutoHideScrollBars = false
                };
                //dialog.ColorScheme = new ColorScheme()
                //{
                //    Normal = new Terminal.Gui.Attribute(Color.White, Color.Black),
                //    Focus = new Terminal.Gui.Attribute(Color.Black, Color.White),
                //    HotFocus = new Terminal.Gui.Attribute(Color.White, Color.Black),
                //    HotNormal = new Terminal.Gui.Attribute(Color.White, Color.Black)
                //};
                int selectedRuleIndex = rulesListview.SelectedItem;
                var selectedRule = ipt.getiptablesRules()[selectedRuleIndex];
                dialog.Add(new Label($"Rule: {(selectedRule.fulllineFromValues).ToString()}")
                {
                    X = 0,
                    Y = 0
                });
                var exitDialog = new Button("Exit")
                {
                    X = Pos.Center() - 13,
                    Y = Pos.Bottom(dialog) - 4
                };
                exitDialog.Clicked += () =>
                {
                    //dialog.Running = false;
                    Application.RequestStop();
                };
                var saveAndExitDialog = new Button("Save and Exit")
                {
                    X = Pos.Center() + 3,
                    Y = Pos.Bottom(dialog) - 4
                };
                if (selectedRule.ruleType == iptablesInterface.Rule.RuleType.ADD || selectedRule.fulllineFromValues == "")
                {
                    var chain = new Label("Chain:")
                    {
                        X = 0,
                        Y = 1
                    };
                    var chainTextbox = new TextField(selectedRule.chain)
                    {
                        X = 30,
                        Y = 1,
                        Width = Dim.Fill(1),
                    };
                    var sourceLabel = new Label("Source:")
                    {
                        X = 0,
                        Y = Pos.Bottom(chain) + 1
                    };
                    var sourceTextbox = new TextField(selectedRule.source)
                    {
                        X = 30,
                        Y = Pos.Bottom(chain) + 1,
                        Width = Dim.Fill(1),
                    };
                    sourceTextbox.CursorPosition = 0; //workaround textbox scroll bug.
                    var destinationLabel = new Label("Destination:")
                    {
                        X = 0,
                        Y = Pos.Bottom(sourceLabel) + 1
                    };
                    var destinationTextbox = new TextField(selectedRule.destination)
                    {
                        X = 30,
                        Y = Pos.Bottom(sourceLabel) + 1,
                        Width = Dim.Fill(1)
                    };
                    destinationTextbox.CursorPosition = 0;
                    var destinationPortLabel = new Label("Destination Port:")
                    {
                        X = 0,
                        Y = Pos.Bottom(destinationTextbox) + 1
                    };
                    var destinationPortTextbox = new TextField(selectedRule.destinationPort)
                    {
                        X = 30,
                        Y = Pos.Bottom(destinationTextbox) + 1,
                        Width = Dim.Fill(1)
                    };
                    var sourcePortLabel = new Label("Source Port:")
                    {
                        X = 0,
                        Y = Pos.Bottom(destinationPortTextbox) + 1
                    };
                    var sourcePortTextbox = new TextField(Text = selectedRule.sourcePort)
                    {
                        X = 30,
                        Y = Pos.Bottom(destinationPortTextbox) + 1,
                        Width = Dim.Fill(1)

                    };
                    var inInterface = new Label("In Interface:")
                    {
                        X = 0,
                        Y = Pos.Bottom(sourcePortTextbox) + 1
                    };
                    var inInterfaceTextbox = new TextField(selectedRule.inInterface)
                    {
                        X = 30,
                        Y = Pos.Bottom(sourcePortTextbox) + 1,
                        Width = Dim.Fill(1)
                    };
                    var outInterface = new Label("Out Interface:")
                    {
                        X = 0,
                        Y = Pos.Bottom(inInterfaceTextbox) + 1
                    };
                    var outInterfaceTextbox = new TextField(selectedRule.outInterface)
                    {
                        X = 30,
                        Y = Pos.Bottom(inInterfaceTextbox) + 1,
                        Width = Dim.Fill(1)
                    };
                    var physicalInInterfaceLabel = new Label("Physical In Interface:")
                    {
                        X = 0,
                        Y = Pos.Bottom(outInterfaceTextbox) + 1
                    };
                    var physicalInInterfaceTextbox = new TextField(selectedRule.physicalInInterface)
                    {
                        X = 30,
                        Y = Pos.Bottom(outInterfaceTextbox) + 1,
                        Width = Dim.Fill(1)
                    };
                    var physicalOutInterfaceLabel = new Label("Physical Out Interface:")
                    {
                        X = 0,
                        Y = Pos.Bottom(physicalInInterfaceTextbox) + 1
                    };
                    var physicalOutInterfaceTextbox = new TextField(selectedRule.physicalOutInterface)
                    {
                        X = 30,
                        Y = Pos.Bottom(physicalInInterfaceTextbox) + 1,
                        Width = Dim.Fill(1)
                    };
                    var protocolLabel = new Label("Protocol:")
                    {
                        X = 0,
                        Y = Pos.Bottom(physicalOutInterfaceTextbox) + 1
                    };
                    var protocolSelect = new ComboBox(selectedRule.protocol)
                    {
                        X = 30,
                        Y = Pos.Bottom(physicalOutInterfaceTextbox) + 1,
                        Width = Dim.Fill(1),
                        Height = 5,
                    };
                    protocolSelect.SetSource(new string[] { "tcp", "udp", "icmp", "" });
                    var targetLabel = new Label("Target:")
                    {
                        X = 0,
                        Y = Pos.Bottom(protocolSelect) -3
                    };
                    var targetSelect = new ComboBox(selectedRule.target)
                    {
                        X = 30,
                        Y = Pos.Bottom(protocolSelect) - 3,
                        Width = Dim.Fill(1),
                        Height = 5
                    };
                    targetSelect.SetSource(new string[] { "ACCEPT", "DROP", "REJECT", "LOG", "MASQUERADE", "RETURN" });
                    var fragmentationLabel = new Label("Fragmentation:")
                    {
                        X = 0,
                        Y = Pos.Bottom(targetSelect) - 3
                    };
                    var fragmentationSelect = new ComboBox(selectedRule.fragmentation)
                    {
                        X = 30,
                        Y = Pos.Bottom(targetSelect) - 3,
                        Width = Dim.Fill(1),
                        Height = 5
                    };
                    fragmentationSelect.SetSource(new string[] { "", "yes", "no" });
                    var tcpflagsLabel = new Label("TCP Flags:")
                    {
                        X = 0,
                        Y = Pos.Bottom(fragmentationSelect) - 3
                    };
                    var tcpflagsTextBox = new TextField(selectedRule.tcpflags) //COMMA SEPERATED LIST!
                    {
                        X = 30,
                        Y = Pos.Bottom(fragmentationSelect) - 3,
                        Width = Dim.Fill(1),
                    };
                    var tcpOptionNumberLabel = new Label("TCP Option Value:")
                    {
                        X = 0,
                        Y = Pos.Bottom(tcpflagsTextBox) + 1
                    };
                    var tcpOptionNumberSelect = new TextField(selectedRule.tcpOptionNumber)
                    {
                        X = 30,
                        Y = Pos.Bottom(tcpflagsTextBox) + 1,
                        Width = Dim.Fill(1)
                    };
                    var icmpTypeLabel = new Label("ICMP Type:")
                    {
                        X = 0,
                        Y = Pos.Bottom(tcpOptionNumberLabel) + 1
                    };
                    var icmpTypeSelect = new ComboBox(selectedRule.icmpType)
                    {
                        X = 30,
                        Y = Pos.Bottom(tcpOptionNumberSelect) + 1,
                        Width = Dim.Fill(1),
                        Height = 5
                    };
                    icmpTypeSelect.SetSource(new string[] { "", "echo-reply", "destination-unreachable", "source-quench", "redirect", "echo-request", "router-advertisement", "router-solicitation", "time-exceeded", "parameter-problem", "timestamp-request", "timestamp-reply", "address-mask-request", "address-mask-reply" });

                    var ethernetAddressLabel = new Label("Ethernet Address:")
                    {
                        X = 0,
                        Y = Pos.Bottom(icmpTypeSelect) - 3
                    };
                    var ethernetAddressTextbox = new TextField(selectedRule.ethernetAddress)
                    {
                        X = 30,
                        Y = Pos.Bottom(icmpTypeSelect) - 3,
                        Width = Dim.Fill(1)
                    };
                    var packetflowRateLabel = new Label("Packetflow Rate:")
                    {
                        X = 0,
                        Y = Pos.Bottom(ethernetAddressTextbox) + 1
                    };
                    var packetflowRateTextbox = new TextField(selectedRule.packetflowRate)
                    {
                        X = 30,
                        Y = Pos.Bottom(ethernetAddressTextbox) + 1,
                        Width = Dim.Fill(1),
                    };
                    var packetburstRateLabel = new Label("Packetburst Rate:")
                    {
                        X = 0,
                        Y = Pos.Bottom(packetflowRateTextbox) + 1
                    };
                    var packetburstRateTextbox = new TextField(selectedRule.packetburstRate)
                    {
                        X = 30,
                        Y = Pos.Bottom(packetflowRateTextbox) + 1,
                        Width = Dim.Fill(1),
                    };
                    var connectionStatesLabel = new Label("Connection States:")
                    {
                        X = 0,
                        Y = Pos.Bottom(packetburstRateTextbox) + 1
                    };
                    var connectionStatesTextBox = new TextField(selectedRule.connectionStates)
                    {
                        X = 30,
                        Y = Pos.Bottom(packetburstRateTextbox) + 1,
                        Width = Dim.Fill(1)
                    };
                    var serviceLabel = new Label("Service:")
                    {
                        X = 0,
                        Y = Pos.Bottom(connectionStatesTextBox) + 1
                    };
                    var serviceTextbox = new TextField(selectedRule.service)
                    {
                        X = 30,
                        Y = Pos.Bottom(connectionStatesTextBox) + 1,
                        Width = Dim.Fill(1)
                    };
                    var packetIncomingBridgeInterfaceLabel = new Label("Packet Incoming on\nBridge Interface:")
                    {
                        X = 0,
                        Y = Pos.Bottom(serviceTextbox) + 1
                    };
                    var packetIncomingBridgeInterfaceComboBox = new ComboBox(selectedRule.packetIncomingBridgeInterface)
                    {
                        X = 30,
                        Y = Pos.Bottom(serviceTextbox) + 2,
                        Width = Dim.Fill(1),
                        Height = 5
                    };
                    packetIncomingBridgeInterfaceComboBox.SetSource(new string[] { "", "yes", "no" });
                    var packetOutgoingBridgeInterfaceLabel = new Label("Packet Outgoing\non Bridge Interface:")
                    {
                        X = 0,
                        Y = Pos.Bottom(packetIncomingBridgeInterfaceComboBox) - 3
                    };
                    var packetOutgoingBridgeInterfaceComboBox = new ComboBox(selectedRule.packetOutgoingBridgeInterface)
                    {
                        X = 30,
                        Y = Pos.Bottom(packetIncomingBridgeInterfaceComboBox) - 2,
                        Width = Dim.Fill(1),
                        Height = 5
                    };
                    packetOutgoingBridgeInterfaceComboBox.SetSource(new string[] { "", "yes", "no"});
                    var packetBeingBridgedLabel = new Label("Packet Being Bridged:")
                    {
                        X = 0,
                        Y = Pos.Bottom(packetOutgoingBridgeInterfaceComboBox) - 3
                    };
                    var packetBeingBridgedComboBox = new ComboBox(selectedRule.packetBeingBridged)
                    {
                        X = 30,
                        Y = Pos.Bottom(packetOutgoingBridgeInterfaceComboBox) - 3,
                        Width = Dim.Fill(1),
                        Height = 5
                    };
                    packetBeingBridgedComboBox.SetSource(new string[] { "", "yes", "no" });
                    var additionalModules = new Label("Additional Modules:")
                    {
                        X = 0,
                        Y = Pos.Bottom(packetBeingBridgedComboBox) - 3
                    };
                    var additionalModulesTextbox = new TextField(selectedRule.additionalModules)
                    {
                        X = 30,
                        Y = Pos.Bottom(packetBeingBridgedComboBox) - 3,
                        Width = Dim.Fill(1)
                    };
                    var additionalParametersLabel = new Label("Additional Parameters:")
                    {
                        X = 0,
                        Y = Pos.Bottom(additionalModulesTextbox) + 1
                    };
                    var additionalParametersTextbox = new TextField(selectedRule.additionalParameters)
                    {
                        X = 30,
                        Y = Pos.Bottom(additionalModulesTextbox) + 1,
                        Width = Dim.Fill(1)
                    };

                    var toDestinationLabel = new Label("To Destination:")
                    {
                        X = 0,
                        Y = Pos.Bottom(additionalParametersTextbox) + 1
                    };
                    var toDestinationTextbox = new TextField()
                    {
                        X = 30,
                        Y = Pos.Bottom(additionalParametersTextbox) + 1,
                        Width = Dim.Fill(1)
                    };
                    var toSourceLabel = new Label("To Source:")
                    {
                        X = 0,
                        Y = Pos.Bottom(toDestinationLabel) + 1
                    };
                    var toSourceTextBox = new TextField()
                    {
                        X = 30,
                        Y = Pos.Bottom(toDestinationLabel) + 1,
                        Width = Dim.Fill(1)

                    };
                    saveAndExitDialog.Clicked += () =>
                    {
                        if (selectedRule is iptablesInterface.NATRule)
                        {
                            ipt.getiptablesRules()[rulesListview.SelectedItem] = new iptablesInterface.NATRule() 
                            {
                                ruleType = iptablesInterface.Rule.RuleType.ADD,
                                chain = chainTextbox.Text.ToString(),
                                protocol = protocolSelect.Text.ToString(),
                                source = sourceTextbox.Text.ToString(),
                                destination = destinationTextbox.Text.ToString(),
                                target = targetSelect.Text.ToString(),
                                outInterface = outInterfaceTextbox.Text.ToString(),
                                inInterface = inInterfaceTextbox.Text.ToString(),
                                destinationPort = destinationPortTextbox.Text.ToString(),
                                sourcePort = sourcePortTextbox.Text.ToString(),
                                physicalInInterface = physicalInInterfaceTextbox.Text.ToString(),
                                physicalOutInterface = physicalOutInterfaceTextbox.Text.ToString(),
                                fragmentation = fragmentationSelect.Text.ToString(),
                                tcpflags = tcpflagsTextBox.Text.ToString(),
                                tcpOptionNumber = tcpOptionNumberSelect.Text.ToString(),
                                icmpType = icmpTypeSelect.Text.ToString(),
                                ethernetAddress = ethernetAddressTextbox.Text.ToString(),
                                packetflowRate = packetflowRateTextbox.Text.ToString(),
                                packetburstRate = packetburstRateTextbox.Text.ToString(),
                                connectionStates = connectionStatesTextBox.Text.ToString(),
                                service = serviceTextbox.Text.ToString(),
                                packetIncomingBridgeInterface = packetIncomingBridgeInterfaceComboBox.Text.ToString(),
                                packetOutgoingBridgeInterface = packetOutgoingBridgeInterfaceComboBox.Text.ToString(),
                                packetBeingBridged = packetBeingBridgedComboBox.Text.ToString(),
                                additionalModules = additionalModulesTextbox.Text.ToString(),
                                additionalParameters = additionalParametersTextbox.Text.ToString(),
                                toDestination = toDestinationTextbox.Text.ToString(),
                                toSource = toSourceTextBox.Text.ToString()
                            };
                        } else 
                        {
                            ipt.getiptablesRules()[rulesListview.SelectedItem] = new iptablesInterface.Rule() 
                            {
                                ruleType = iptablesInterface.Rule.RuleType.ADD,
                                chain = chainTextbox.Text.ToString(),
                                protocol = protocolSelect.Text.ToString(),
                                source = sourceTextbox.Text.ToString(),
                                destination = destinationTextbox.Text.ToString(),
                                target = targetSelect.Text.ToString(),
                                outInterface = outInterfaceTextbox.Text.ToString(),
                                inInterface = inInterfaceTextbox.Text.ToString(),
                                destinationPort = destinationPortTextbox.Text.ToString(),
                                sourcePort = sourcePortTextbox.Text.ToString(),
                                physicalInInterface = physicalInInterfaceTextbox.Text.ToString(),
                                physicalOutInterface = physicalOutInterfaceTextbox.Text.ToString(),
                                fragmentation = fragmentationSelect.Text.ToString(),
                                tcpflags = tcpflagsTextBox.Text.ToString(),
                                tcpOptionNumber = tcpOptionNumberSelect.Text.ToString(),
                                icmpType = icmpTypeSelect.Text.ToString(),
                                ethernetAddress = ethernetAddressTextbox.Text.ToString(),
                                packetflowRate = packetflowRateTextbox.Text.ToString(),
                                packetburstRate = packetburstRateTextbox.Text.ToString(),
                                connectionStates = connectionStatesTextBox.Text.ToString(),
                                service = serviceTextbox.Text.ToString(),
                                packetIncomingBridgeInterface = packetIncomingBridgeInterfaceComboBox.Text.ToString(),
                                packetOutgoingBridgeInterface = packetOutgoingBridgeInterfaceComboBox.Text.ToString(),
                                packetBeingBridged = packetBeingBridgedComboBox.Text.ToString(),
                                additionalModules = additionalModulesTextbox.Text.ToString(),
                                additionalParameters = additionalParametersTextbox.Text.ToString()
                            };
                        }
                        //dialog.Running = false;
                        rulesListview.SetSource(ipt.getiptablesRules().Select(x => x.fulllineFromValues).ToList());
                        rulesListview.SelectedItem = selectedRuleIndex;
                        Application.RequestStop();
                    };
                    scrollview.Add(chain);
                    scrollview.Add(chainTextbox);
                    scrollview.Add(sourceLabel);
                    scrollview.Add(sourceTextbox);
                    scrollview.Add(destinationLabel);
                    scrollview.Add(destinationTextbox);
                    scrollview.Add(destinationPortLabel);
                    scrollview.Add(destinationPortTextbox);
                    scrollview.Add(sourcePortLabel);
                    scrollview.Add(sourcePortTextbox);
                    scrollview.Add(inInterface);
                    scrollview.Add(inInterfaceTextbox);
                    scrollview.Add(outInterface);
                    scrollview.Add(outInterfaceTextbox);
                    scrollview.Add(physicalInInterfaceLabel);
                    scrollview.Add(physicalInInterfaceTextbox);
                    scrollview.Add(physicalOutInterfaceLabel);
                    scrollview.Add(physicalOutInterfaceTextbox);
                    scrollview.Add(protocolLabel);
                    scrollview.Add(protocolSelect);
                    scrollview.Add(targetLabel);
                    scrollview.Add(targetSelect);
                    scrollview.Add(fragmentationLabel);
                    scrollview.Add(fragmentationSelect);
                    scrollview.Add(tcpflagsLabel);
                    scrollview.Add(tcpflagsTextBox);
                    scrollview.Add(tcpOptionNumberLabel);
                    scrollview.Add(tcpOptionNumberSelect);
                    scrollview.Add(icmpTypeLabel);
                    scrollview.Add(icmpTypeSelect);
                    scrollview.Add(ethernetAddressLabel);
                    scrollview.Add(ethernetAddressTextbox);
                    scrollview.Add(packetflowRateLabel);
                    scrollview.Add(packetflowRateTextbox);
                    scrollview.Add(packetburstRateLabel);
                    scrollview.Add(packetburstRateTextbox);
                    scrollview.Add(connectionStatesLabel);
                    scrollview.Add(connectionStatesTextBox);
                    scrollview.Add(serviceLabel);
                    scrollview.Add(serviceTextbox);
                    scrollview.Add(packetIncomingBridgeInterfaceLabel);
                    scrollview.Add(packetIncomingBridgeInterfaceComboBox);
                    scrollview.Add(packetOutgoingBridgeInterfaceLabel);
                    scrollview.Add(packetOutgoingBridgeInterfaceComboBox);
                    scrollview.Add(packetBeingBridgedLabel);
                    scrollview.Add(packetBeingBridgedComboBox);
                    scrollview.Add(additionalModules);
                    scrollview.Add(additionalModulesTextbox);
                    scrollview.Add(additionalParametersLabel);
                    scrollview.Add(additionalParametersTextbox);
                    if(selectedRule is iptablesInterface.NATRule)
                    {
                        targetSelect.SetSource(new List<string>() { "DNAT", "SNAT", "MASQUERADE" });
                        toDestinationTextbox.Text = selectedRule.toDestination;
                        toSourceTextBox.Text = selectedRule.toSource;
                        scrollview.Add(toDestinationLabel);
                        scrollview.Add(toDestinationTextbox);
                        scrollview.Add(toSourceLabel);
                        scrollview.Add(toSourceTextBox);
                    }
                    dialog.Add(scrollview);

                    //scrollview.Add(view);
                    dialog.Add(exitDialog);
                    dialog.Add(saveAndExitDialog);
                    Application.Run(dialog);
                }
                else
                {
                    //dialog.Running = false;
                    editmanualRuleDialog();
                }
            };

            #endregion


            #region addbuttonDialog
                 add.Clicked += () =>
                 {
                        int newRuleIndex = ipt.reorderRule(ipt.addRule(""), rulesListview.SelectedItem);
                        rulesListview.SetSource(ipt.getiptablesRules().Select(x => x.fulllineFromValues).ToList());
                        rulesListview.SelectedItem = newRuleIndex;
                        var dialog = new Dialog("Add Rule")
                        {
                            X = Pos.Center(),
                            Y = Pos.Center()
                        };
                        //dialog.ColorScheme = new ColorScheme()
                        //{
                        //    Normal = new Terminal.Gui.Attribute(Color.White, Color.Black),
                        //    Focus = new Terminal.Gui.Attribute(Color.Black, Color.White),
                        //    HotFocus = new Terminal.Gui.Attribute(Color.White, Color.Black),
                        //    HotNormal = new Terminal.Gui.Attribute(Color.White, Color.Black)
                        //};
                        var ManualButton = new Button("Type a rule manually using syntax")
                        {
                            X = Pos.Center(),
                            Y = Pos.Center()-3,
                            Width = Dim.Fill(5),
                            Height = 1
                        };
                        ManualButton.Clicked += () =>
                        {
                            //dialog.Running = false;
                            editmanualRuleDialog();
                            Application.RequestStop();
                        };
                        var PortRule = new Button("Create a port rule using the port rule editor")
                        {
                            X = Pos.Center(),
                            Y = Pos.Bottom(ManualButton) + 1,
                            Width = Dim.Fill(5),
                            Height = 1
                        };
                        PortRule.Clicked += () =>
                        {
                            //dialog.Running = false;
                            Application.RequestStop();
                            editPortRuleDialog();
                        };
                        dialog.Add(ManualButton);
                        dialog.Add(PortRule);
                        Application.Run(dialog);

                 };
            #endregion
            del.Clicked += () =>
            {
                int selectedRuleIndex = rulesListview.SelectedItem;
                ipt.removeRule(selectedRuleIndex);
                rulesListview.SetSource(ipt.getiptablesRules().Select(x => x.fulllineFromValues).ToList());
                rulesListview.SelectedItem = (selectedRuleIndex == 0) ? 0 : selectedRuleIndex - 1;
            };
            save.Clicked += () =>
            {
                List<string> failedrules = ipt.applyRules();
                if (failedrules.Count > 0)
                {
                    var dialog = new Dialog("Failed to apply rules")
                    {
                        X = Pos.Center(),
                        Y = Pos.Center(),
                        Height = Dim.Fill() - 5
                    };
                    //dialog.ColorScheme = new ColorScheme()
                    //{
                    //    Normal = new Terminal.Gui.Attribute(Color.Gray, Color.Black),
                    //    Focus = new Terminal.Gui.Attribute(Color.Black, Color.White),
                    //    HotFocus = new Terminal.Gui.Attribute(Color.White, Color.Black),
                    //    HotNormal = new Terminal.Gui.Attribute(Color.White, Color.Black)
                    //};
                    var exitDialog = new Button("Exit")
                    {
                        X = Pos.Center(),
                        Y = Pos.Bottom(dialog) - 5,
                        Width = Dim.Fill(5),
                        Height = 1
                    };
                    exitDialog.Clicked += () =>
                    {
                        //dialog.Running = false;
                        Application.RequestStop();

                    };
                    var failedRulesText = new TextView()
                    {
                        X = 0,
                        Y = 0,
                        Width = Dim.Fill(1),
                        Height = Dim.Fill(1) - 8,
                        Text = string.Join("\n", failedrules),
                        Enabled = false
                    };
                    dialog.Add(failedRulesText);
                    dialog.Add(exitDialog);
                    Application.Run(dialog);
                }
                else
                {
                    var dialog = new Dialog("Successfully applied rules")
                    {
                        X = Pos.Center(),
                        Y = Pos.Center(),
                        Height = Dim.Fill() - 5
                    };
                    //dialog.ColorScheme = new ColorScheme()
                    //{
                    //    Normal = new Terminal.Gui.Attribute(Color.White, Color.Black),
                    //    Focus = new Terminal.Gui.Attribute(Color.Black, Color.White),
                    //    HotFocus = new Terminal.Gui.Attribute(Color.White, Color.Black),
                    //    HotNormal = new Terminal.Gui.Attribute(Color.White, Color.Black)
                    //};
                    var exitDialog = new Button("Exit")
                    {
                        X = Pos.Center(),
                        Y = Pos.Bottom(dialog) - 8,
                        Width = Dim.Fill(5),
                        Height = 1
                    };
                    exitDialog.Clicked += () =>
                    {
                        //dialog.Running = false;
                        Application.RequestStop();

                    };
                    dialog.Add(exitDialog);
                    Application.Run(dialog);
                }

            };
            edit.Clicked += editPortRuleDialog;
            manualedit.Clicked += editmanualRuleDialog;
            
            Add(new Label("Rules"));
            Add(rulesListview);
            Add(quit);
            Add(edit);
            Add(manualedit);
            Add(add);
            Add(del);
            Add(save);
            Add(moveRuleUp);
            Add(moveRuleDown);

        }
    }
}