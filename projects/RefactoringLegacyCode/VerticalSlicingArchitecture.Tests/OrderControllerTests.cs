using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using FsCheck;

namespace RefactoringLegacyCode.Tests
{
    public class OrderControllerTests
    {
        [Test]
        public void asd()
        {
            new OrdersController().ProcessOrder(1);
            true.Should().BeTrue();
        }
    }
}
