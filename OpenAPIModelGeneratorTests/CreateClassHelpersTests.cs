using OpenAPIModelGenerator.Models;

namespace OpenAPIModelGeneratorTests
{
    public class CreateClassHelpersTests
    {
        public static readonly (string testString, string? expectedString)[] MemberNameData =
    [
        ("", string.Empty),
        ("id","Id"),
        ("name","Name"),
        ("client_id","ClientId"),
        ("estimated_worker_count","EstimatedWorkerCount"),
        ("monthly-estimated-revenue","MonthlyEstimatedRevenue"),
        ("accounts$receivable","AccountsReceivable"),
        ("accounts!@&&$receivable","AccountsReceivable"),
        ("123abc", "Abc"),  // Starts with number
        ("  leadingAndTrailing  ", "Leadingandtrailing"),  // Leading and trailing spaces
        ("foo_bar-baz", "FooBarBaz"),  // Special characters
        ("foo___bar", "FooBar"),  // Consecutive special chars
        ("HELLO_WORLD", "HelloWorld"),  // Uppercase string
        ("user_2nd_version", "User2ndVersion"),  // Numeric values
        ("a", "A"),  // Single character
        ("first_name-last_name", "FirstNameLastName")  // Mixed delimiters
    ];

        [TestCaseSource(nameof(MemberNameData))]
        public void CreateMemberOrClassName_ReceivePascalFormatString((string testString, string expectedResult) data)
        {
            var result = CreateClassHelpers.CreateMemberOrClassName(data.testString);
            Assert.That(result, Is.EqualTo(data.expectedResult));
        }
    }
}