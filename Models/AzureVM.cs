using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Resources.Models;
using Azure.ResourceManager.Compute;
using Azure.ResourceManager.Compute.Models;
using Azure.ResourceManager.Network;
using Azure.ResourceManager.Network.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Azure.ResourceManager;
using Azure;

namespace test.Models
{
    public static class AzureVM
    {
        public static VirtualMachineResource CreateAzureVM(string VMPwd)
        {
            string resourceGroupName = "pvp-rg";
            string publicIpName = "pvp-ip";
            string virtualNetworkName = "pvp-vn";
            string networkInterfaceName = "pvp-nic";
            string vmName = "pvp-vm";
            string computeName = "computeName";
            string admin = "pvpvmadmin";
            string pwd = VMPwd;

            var client = new ArmClient(new DefaultAzureCredential());
            ResourceGroupResource resourceGroup = client.GetDefaultSubscription().GetResourceGroup(resourceGroupName);
            VirtualMachineCollection vms = resourceGroup.GetVirtualMachines();
            NetworkInterfaceCollection nics = resourceGroup.GetNetworkInterfaces();
            VirtualNetworkCollection vns = resourceGroup.GetVirtualNetworks();
            PublicIPAddressCollection publicIps = resourceGroup.GetPublicIPAddresses();
            NetworkSecurityGroupCollection nsgc = resourceGroup.GetNetworkSecurityGroups();

            PublicIPAddressResource ipResource = publicIps.CreateOrUpdate(
                WaitUntil.Completed,
                publicIpName,
                new PublicIPAddressData()
                {
                    PublicIPAddressVersion = NetworkIPVersion.IPv4,
                    PublicIPAllocationMethod = NetworkIPAllocationMethod.Static,
                    Location = AzureLocation.NorthEurope
                }).Value;

                        NetworkSecurityGroupResource nsgr = nsgc.CreateOrUpdate(
                WaitUntil.Completed,
                "pvp-nsg",
                new NetworkSecurityGroupData
                {
                    Location = AzureLocation.NorthEurope,
                    SecurityRules = {
                        new SecurityRuleData
                        {
                            Name = "AllowSSH",
                            Priority = 700,
                            Access = SecurityRuleAccess.Allow,
                            Direction = SecurityRuleDirection.Inbound,
                            SourceAddressPrefix = "*",
                            SourcePortRange = "*",
                            DestinationAddressPrefix = "*",
                            DestinationPortRange = "22",
                            Protocol = SecurityRuleProtocol.Tcp
                        },
                        new SecurityRuleData
                        {
                            Name = "AllowRabbitMQ",
                            Priority = 710,
                            Access = SecurityRuleAccess.Allow,
                            Direction = SecurityRuleDirection.Inbound,
                            SourceAddressPrefix = "*",
                            SourcePortRange = "*",
                            DestinationAddressPrefix = "*",
                            DestinationPortRanges = {
                                "5672",
                                "15672"
                            },
                            Protocol = SecurityRuleProtocol.Asterisk
                        }
                    }
                }
            ).Value;

            VirtualNetworkResource vnetResrouce = vns.CreateOrUpdate(
                WaitUntil.Completed,
                virtualNetworkName,
                new VirtualNetworkData()
                {
                    Location = AzureLocation.NorthEurope,
                    Subnets =
                    {
                        new SubnetData()
                        {
                            Name = "testSubNet",
                            AddressPrefix = "10.0.0.0/24",
                            NetworkSecurityGroup = nsgr.Data
                        }
                    },
                    AddressPrefixes =
                    {
                        "10.0.0.0/16"
                    },
                }).Value;

            NetworkInterfaceResource nicResource = nics.CreateOrUpdate(
                WaitUntil.Completed,
                networkInterfaceName,
                new NetworkInterfaceData()
                {
                    Location = AzureLocation.NorthEurope,
                    IPConfigurations =
                    {
                        new NetworkInterfaceIPConfigurationData()
                        {
                            Name = "Primary",
                            Primary = true,
                            Subnet = new SubnetData() { Id = vnetResrouce?.Data.Subnets.First().Id },
                            PrivateIPAllocationMethod = NetworkIPAllocationMethod.Dynamic,
                            PublicIPAddress = new PublicIPAddressData() { Id = ipResource?.Data.Id }
                        }
                    }
                }).Value;

                VirtualMachineResource vmResource = vms.CreateOrUpdate(
                WaitUntil.Completed,
                vmName,
                new VirtualMachineData(AzureLocation.NorthEurope)
                {
                    HardwareProfile = new VirtualMachineHardwareProfile()
                    {
                        VmSize = VirtualMachineSizeType.StandardDS1V2
                    },
                    OSProfile = new VirtualMachineOSProfile()
                    {
                        ComputerName = computeName,
                        AdminUsername = admin,
                        AdminPassword = pwd,
                        LinuxConfiguration = new LinuxConfiguration() 
                        { 
                            DisablePasswordAuthentication = false, 
                            ProvisionVmAgent = true 
                        }
                    },
                    StorageProfile = new VirtualMachineStorageProfile()
                    {
                        OSDisk = new VirtualMachineOSDisk(DiskCreateOptionType.FromImage),
                        ImageReference = new ImageReference()
                        {
                            Offer = "UbuntuServer",
                            Publisher = "Canonical",
                            Sku = "18.04-LTS",
                            Version = "latest"
                        }
                    },
                    NetworkProfile = new VirtualMachineNetworkProfile()
                    {
                        NetworkInterfaces =
                        {
                            new VirtualMachineNetworkInterfaceReference()
                            {
                                Id = nicResource.Id
                            }
                        }
                    },
                }).Value;
                return vmResource;
        }
    }
}