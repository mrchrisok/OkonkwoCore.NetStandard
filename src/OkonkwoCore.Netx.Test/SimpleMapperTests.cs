using OkonkwoCore.Netx.Utilities;
using Xunit;

namespace OkonkwoCore.Netx.Test
{
    public class SimpleMapperTests
    {
        [Fact]
        public void PropertyMap_SameTypeGiven_TargetPropertiesShouldMatchSource()
        {
            // arrange
            var testPerson1 = new TestPerson() { FirstName = "Mike", LastName = "Tyson" };
            var testPerson2 = new TestPerson();

            // act
            SimpleMapper.PropertyMap(testPerson1, testPerson2);

            // assert
            Assert.True(testPerson1.FirstName == testPerson2.FirstName);
            Assert.True(testPerson1.LastName == testPerson2.LastName);
        }
    }

    public class TestPerson
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
