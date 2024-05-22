namespace TiCodeX.SQLSchemaCompare.Test.Core.Exceptions
{
    using System;
    using TiCodeX.SQLSchemaCompare.Core.Entities.Exceptions;
    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;

    /// <summary>
    /// Test class for the PropertyNotFoundException class
    /// </summary>
    public class PropertyNotFoundExceptionTest : BaseTests<PropertyNotFoundExceptionTest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyNotFoundExceptionTest"/> class.
        /// </summary>
        /// <param name="output">The test output helper</param>
        public PropertyNotFoundExceptionTest(ITestOutputHelper output)
            : base(output)
        {
        }

        /// <summary>
        /// Test the retrieval of database list with all the databases
        /// </summary>
        [Fact]
        [UnitTest]
        public void ExceptionMessage()
        {
            var ex = new PropertyNotFoundException(typeof(PropertyNotFoundException), "Test");
            Assert.True(ex.ClassType != null && ex.PropertyName == "Test");
            Assert.Equal($"The property has not been found in the class.{Environment.NewLine}Class: PropertyNotFoundException; Property: Test", ex.Message);

            ex = new PropertyNotFoundException("Test message", typeof(PropertyNotFoundException), "Test");
            Assert.True(ex.ClassType != null && ex.PropertyName == "Test");
            Assert.Equal($"Test message{Environment.NewLine}Class: PropertyNotFoundException; Property: Test", ex.Message);

            ex = new PropertyNotFoundException("Test message 2", typeof(PropertyNotFoundException), "Test", new InvalidOperationException());
            Assert.True(ex.ClassType != null && ex.PropertyName == "Test");
            Assert.True(ex.InnerException != null);
            Assert.Equal($"Test message 2{Environment.NewLine}Class: PropertyNotFoundException; Property: Test", ex.Message);

            ex = new PropertyNotFoundException("Test message 3");
            Assert.True(ex.ClassType == null && ex.PropertyName == null);
            Assert.Equal("Test message 3", ex.Message);
        }
    }
}
