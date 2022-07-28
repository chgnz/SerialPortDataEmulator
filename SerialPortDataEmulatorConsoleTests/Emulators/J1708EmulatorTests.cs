using Microsoft.VisualStudio.TestTools.UnitTesting;
using SerialPortDataEmulatorConsole.SerialProtocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialPortDataEmulatorConsole.SerialProtocols.Tests
{
    [TestClass()]
    public class J1708EmulatorTests
    {
        [TestMethod()]
        public void j1708_checksumTest()
        {
            J1708Emulator j1708 = new J1708Emulator();
            byte[] message_without_checksum;
            byte checksum;

            message_without_checksum = new byte[] { 128, 21, 50, 12, 5, 48 };
            checksum = j1708.j1708_checksum(message_without_checksum);
            Assert.AreEqual(248, checksum);

            message_without_checksum = new byte[] { 128, 95, 23, 45, 123 };
            checksum = j1708.j1708_checksum(message_without_checksum);
            Assert.AreEqual(98, checksum);
        }
    }
}