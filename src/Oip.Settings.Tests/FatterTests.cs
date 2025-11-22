using System.Globalization;
using Oip.Settings.Attributes;
using Oip.Settings.Helpers;

namespace Oip.Settings.Tests;

[TestFixture]
public class FatterTests
{
    [Test]
    public void ToDictionary_WithSimpleTypes_ShouldFlattenCorrectly()
    {
        // Arrange
        var instance = new SimpleTypesExample
        {
            TestInt = 42,
            TestDouble = 3.14,
            TestString = "Hello World",
            TestBool = true
        };
        var dictionary = Flatter.ToDictionary(instance);

        // Act
        Assert.Multiple(() =>
        {
            Assert.That(dictionary["TestInt"], Is.EqualTo("42"));
            Assert.That(dictionary["TestDouble"], Is.EqualTo("3.14"));
            Assert.That(dictionary["TestString"], Is.EqualTo("Hello World"));
            Assert.That(dictionary["TestBool"], Is.EqualTo("True"));
        });
    }

    [Test]
    public void ToDictionary_WithStringList_ShouldFlattenWithIndexes()
    {
        // Arrange
        var instance = new ListExample
        {
            TestStringList = ["apple", "banana", "cherry"]
        };

        // Act
        var dictionary = Flatter.ToDictionary(instance);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(dictionary["TestStringList:0"], Is.EqualTo("apple"));
            Assert.That(dictionary["TestStringList:1"], Is.EqualTo("banana"));
            Assert.That(dictionary["TestStringList:2"], Is.EqualTo("cherry"));
        });
    }

    [Test]
    public void ToDictionary_WithObjectList_ShouldFlattenNestedObjects()
    {
        // Arrange
        var instance = new ObjectListExample
        {
            TestObjectList =
            [
                new() { TestInt = 1, TestString = "First" },
                new() { TestInt = 2, TestString = "Second" }
            ]
        };

        // Act
        var dictionary = Flatter.ToDictionary(instance);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(dictionary["TestObjectList:0:TestInt"], Is.EqualTo("1"));
            Assert.That(dictionary["TestObjectList:0:TestString"], Is.EqualTo("First"));
            Assert.That(dictionary["TestObjectList:1:TestInt"], Is.EqualTo("2"));
            Assert.That(dictionary["TestObjectList:1:TestString"], Is.EqualTo("Second"));
        });
    }

    [Test]
    public void ToDictionary_WithDictionary_ShouldFlattenKeyValuePairs()
    {
        // Arrange
        var instance = new DictionaryExample
        {
            TestDictionary = new Dictionary<string, string>
            {
                ["key1"] = "value1",
                ["key2"] = "value2",
                ["language"] = "C#"
            }
        };

        // Act
        var dictionary = Flatter.ToDictionary(instance);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(dictionary["TestDictionary:key1"], Is.EqualTo("value1"));
            Assert.That(dictionary["TestDictionary:key2"], Is.EqualTo("value2"));
            Assert.That(dictionary["TestDictionary:language"], Is.EqualTo("C#"));
        });
    }

    [Test]
    public void ToDictionary_WithNotSaveToDbAttribute_ShouldSkipProperty()
    {
        // Arrange
        var instance = new WithNotSaveAttributeExample
        {
            SaveProperty = "ShouldSave",
            DontSaveProperty = "ShouldNotSave"
        };

        // Act
        var dictionary = Flatter.ToDictionary(instance);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(dictionary.ContainsKey("SaveProperty"), Is.True);
            Assert.That(dictionary.ContainsKey("DontSaveProperty"), Is.False);
            Assert.That(dictionary["SaveProperty"], Is.EqualTo("ShouldSave"));
        });
    }

    [Test]
    public void ToDictionary_WithNullValues_ShouldSkipNullProperties()
    {
        // Arrange
        var instance = new WithNullValuesExample
        {
            NotNullProperty = "HasValue",
            NullProperty = null
        };

        // Act
        var dictionary = Flatter.ToDictionary(instance);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(dictionary.ContainsKey("NotNullProperty"), Is.True);
            Assert.That(dictionary.ContainsKey("NullProperty"), Is.False);
        });
    }

    [Test]
    public void ToDictionary_WithEmptyCollections_ShouldHandleGracefully()
    {
        // Arrange
        var instance = new EmptyCollectionsExample
        {
            EmptyList = new List<string>(),
            EmptyDictionary = new Dictionary<string, string>(),
            ValidProperty = "Valid"
        };

        // Act
        var dictionary = Flatter.ToDictionary(instance);

        // Assert
        Assert.That(dictionary.ContainsKey("ValidProperty"), Is.True);
        Assert.That(dictionary["ValidProperty"], Is.EqualTo("Valid"));
        // Empty collections should not add any entries
    }

    [Test]
    public void ToDictionary_WithNestedComplexObject_ShouldFlattenRecursively()
    {
        // Arrange
        var instance = new ComplexExample
        {
            Simple = new SimpleTypesExample { TestInt = 999, TestString = "Nested" },
            List = new ListExample { TestStringList = new List<string> { "nested1", "nested2" } }
        };

        // Act
        var dictionary = Flatter.ToDictionary(instance);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(dictionary["Simple:TestInt"], Is.EqualTo("999"));
            Assert.That(dictionary["Simple:TestString"], Is.EqualTo("Nested"));
            Assert.That(dictionary["List:TestStringList:0"], Is.EqualTo("nested1"));
            Assert.That(dictionary["List:TestStringList:1"], Is.EqualTo("nested2"));
        });
    }

    [Test]
    public void ToDictionary_WithMixedPrimitivesInList_ShouldHandleCorrectly()
    {
        // Arrange
        var instance = new MixedListExample
        {
            MixedList = new List<object> { "string", 42, 3.14, true }
        };

        // Act
        var dictionary = Flatter.ToDictionary(instance);

        Assert.That(dictionary["MixedList:0"], Is.EqualTo("string"));
        Assert.That(dictionary["MixedList:1"], Is.EqualTo("42"));
        Assert.That(dictionary["MixedList:2"], Is.EqualTo("3.14"));
        Assert.That(dictionary["MixedList:3"], Is.EqualTo("True"));
    }

    [Test]
    public void ToDictionary_WithDateTimeAndOtherSimpleTypes_ShouldHandleCorrectly()
    {
        // Arrange
        var fixedDate = new DateTime(2023, 12, 31, 10, 30, 0);
        var instance = new DateTimeExample
        {
            TestDateTime = fixedDate,
            TestGuid = Guid.Parse("12345678-1234-1234-1234-123456789012"),
            TestDecimal = 123.45m
        };

        // Act
        var dictionary = Flatter.ToDictionary(instance);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(dictionary["TestDateTime"], Is.EqualTo(fixedDate.ToString(CultureInfo.InvariantCulture)));
            Assert.That(dictionary["TestGuid"], Is.EqualTo("12345678-1234-1234-1234-123456789012"));
            Assert.That(dictionary["TestDecimal"], Is.EqualTo("123.45"));
        });
    }

    [Test]
    public void ToDictionary_WithEnum_ShouldFlattenCorrectly()
    {
        var instance = new { Status = Status.Active };

        var dictionary = Flatter.ToDictionary(instance);

        Assert.That(dictionary["Status"], Is.EqualTo("Active"));
    }

    [Test]
    public void ToDictionary_WithNestedDictionary_ShouldFlattenCorrectly()
    {
        var instance = new
        {
            NestedDict = new Dictionary<string, object>
            {
                ["inner"] = new { Value = "test" }
            }
        };

        var dictionary = Flatter.ToDictionary(instance);

        // Проверка вложенной структуры

        Assert.Multiple(() =>
        {
            Assert.That(dictionary.Count, Is.EqualTo(1));
            Assert.That(dictionary["NestedDict:inner:Value"], Is.EqualTo("test"));
        });
    }

    [Test]
    public void ToDictionary_WithNonStringDictionaryKeys_ShouldHandleCorrectly()
    {
        var instance = new
        {
            IntKeyDict = new Dictionary<int, string>
            {
                [1] = "one",
                [2] = "two"
            }
        };
        var dictionary = Flatter.ToDictionary(instance);

        Assert.That(dictionary["IntKeyDict:1"], Is.EqualTo("one"));
    }

    [Test]
    public void ToDictionary_WithCircularReference_ShouldHandleGracefully()
    {
        var obj1 = new CircularExample();
        var obj2 = new CircularExample();
        obj1.Reference = obj2;
        obj2.Reference = obj1;

        Assert.Throws<InvalidOperationException>(() => { _ = Flatter.ToDictionary(obj1); });
    }

    [Test]
    public void ToDictionary_WithBoundaryValues_ShouldHandleCorrectly()
    {
        var instance = new
        {
            MaxInt = int.MaxValue,
            MinInt = int.MinValue,
            MaxDouble = double.MaxValue,
            TrueBool = true,
            FalseBool = false,
            EmptyString = ""
        };
        var dictionary = Flatter.ToDictionary(instance);

        // Проверка всех граничных значений

        Assert.Multiple(() =>
        {
            Assert.That(dictionary["MaxInt"], Is.EqualTo(int.MaxValue.ToString(CultureInfo.InvariantCulture)));
            Assert.That(dictionary["MinInt"], Is.EqualTo(int.MinValue.ToString(CultureInfo.InvariantCulture)));
            Assert.That(dictionary["MaxDouble"], Is.EqualTo(double.MaxValue.ToString(CultureInfo.InvariantCulture)));
            Assert.That(dictionary["TrueBool"], Is.EqualTo(true.ToString()));
            Assert.That(dictionary["FalseBool"], Is.EqualTo(false.ToString()));
            Assert.That(dictionary["EmptyString"], Is.EqualTo(string.Empty));
        });
    }

    [Test]
    public void ToDictionary_WithDateTimeFormats_ShouldUseInvariantCulture()
    {
        var instance = new
        {
            Date = new DateTime(2023, 12, 31),
            DateTime = new DateTime(2023, 12, 31, 23, 59, 59),
            DateTimeOffset = new DateTimeOffset(2023, 12, 31, 23, 59, 59, TimeSpan.Zero),
            TimeSpan = TimeSpan.FromHours(2.5)
        };
        
        var dictionary = Flatter.ToDictionary(instance);


        // Проверка что все форматы используют инвариантную культуру
        Assert.Multiple(() =>
        {
            Assert.That(dictionary["Date"], Is.EqualTo(instance.Date.ToString(CultureInfo.InvariantCulture)));
            Assert.That(dictionary["DateTime"], Is.EqualTo(instance.DateTime.ToString(CultureInfo.InvariantCulture)));
            Assert.That(dictionary["DateTimeOffset"],
                Is.EqualTo(instance.DateTimeOffset.ToString(CultureInfo.InvariantCulture)));
            Assert.That(dictionary["TimeSpan"], Is.EqualTo(instance.TimeSpan.ToString()));
        });
    }

    private class CircularExample
    {
        public CircularExample Reference { get; set; }
        public int Value { get; set; } = 1;
    }

    // Test Classes
    private class SimpleTypesExample
    {
        public int TestInt { get; set; }
        public double TestDouble { get; set; }
        public string TestString { get; set; } = "";
        public bool TestBool { get; set; }
    }

    private class ListExample
    {
        public List<string> TestStringList { get; set; } = new();
    }

    private class ObjectListExample
    {
        public List<SimpleTypesExample> TestObjectList { get; set; } = new();
    }

    private class DictionaryExample
    {
        public Dictionary<string, string> TestDictionary { get; set; } = new();
    }

    private class WithNotSaveAttributeExample
    {
        public string SaveProperty { get; set; } = "";

        [NotSaveToDb] public string DontSaveProperty { get; set; } = "";
    }

    private class WithNullValuesExample
    {
        public string NotNullProperty { get; set; } = "";
        public string? NullProperty { get; set; }
    }

    private class EmptyCollectionsExample
    {
        public List<string> EmptyList { get; set; } = new();
        public Dictionary<string, string> EmptyDictionary { get; set; } = new();
        public string ValidProperty { get; set; } = "";
    }

    private class ComplexExample
    {
        public SimpleTypesExample Simple { get; set; } = new();
        public ListExample List { get; set; } = new();
    }

    private class MixedListExample
    {
        public List<object> MixedList { get; set; } = new();
    }

    private class DateTimeExample
    {
        public DateTime TestDateTime { get; set; }
        public Guid TestGuid { get; set; }
        public decimal TestDecimal { get; set; }
    }


    private class FlatterExample
    {
        public int TestInt { get; set; } = 42;
        public double TestDouble { get; set; } = 2.71828;
        public string TestString { get; set; } = "Default string value";
        public List<string> TestStringList { get; set; } = ["first", "second", "third"];

        public List<FlatterExampleWithoutInnerObject> TestObjectList { get; set; } =
            [new FlatterExampleWithoutInnerObject()];

        public Dictionary<string, string> TestDictionary { get; set; } = new Dictionary<string, string>
        {
            ["name"] = "FlatterExample",
            ["version"] = "1.0",
            ["status"] = "active"
        };
    }

    private class FlatterExampleWithoutInnerObject
    {
        public int TestInt { get; set; } = 42;
        public double TestDouble { get; set; } = 2.71828;
        public string TestString { get; set; } = "Default string value";
        public List<string> TestStringList { get; set; } = ["first", "second", "third"];

        public Dictionary<string, string> TestDictionary { get; set; } = new()
        {
            ["name"] = "FlatterExample",
            ["version"] = "1.0",
            ["status"] = "active"
        };
    }

    private enum Status
    {
        Active,
        Inactive
    }
}