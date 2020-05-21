# ConfigTextFile
A .NET library which allows you to load plain-text configuration files.

## Overview
There are three different kinds of elements in this config file; Sections, Comments, and Key/Value pairs.
Sections are denoted by a name and braces.
Comments are preceded by #.
Key/Value pairs are delimited with the equals sign, and Values can either be a single string, or an array of strings.

## Usage
The ConfigFile class represents a single loaded file. To load one, use the static method TryLoadFile. It accepts either a stream, or string and Encoding.
Once it's loaded, you can either use ConfigFile.GetElement("key") to retrieve parts of the file, or use the IConfiguration interface i.e. ConfigFile.GetSection("key")
You can also use the ConfigFile.Elements Dictionary, but it's inconvenient for reading arrays.

```csharp
LoadResult result = ConfigFile.TryLoadFile("MyFile.cfg", Encoding.UTF8);
if(result.Success)
{
	ConfigFile file = result.ConfigTextFile;

	// We can interpret it as an IConfigurationSection by using GetSection
	string myString = file.GetSection("SomeSection:SomeKey").Value;

	// We can just directly get the string this way. This is less verbose, but throws exceptions when keys are not found or are not a single string
	myString = file["SomeSection:SomeKey"];

	// And finally this lets us use the IConfigElement interface
	IConfigElement singleString = file.GetElement("SomeSection:SomeKey");

	// Throws an exception if the above key was not found
	singleString.ThrowIfInvalid();

	// This returns an empty string if it's not a single string
	myString = singleString.Value;

	IConfigElement array = file.Elements["SomeArray"];
	// We can iterate over each string in the array by doing this
	foreach(IConfigElement elem = array.Elements.Values)
	{
		Console.WriteLine("Array element: " + elem.Value);
	}

	// Can get the sections, and loop over everything they have
	IConfigElement section = file.Elements["SomeSection"];
	foreach(IConfigElement child = section.Elements.Values)
	{
		// The Type member denotes what this is; it's either String, Array, or Section. Types of Invalid are only returned by GetSection or GetElement, so we don't need to worry about that here.
		switch(child.Type)
		{
			case ConfigElementType.String:
				// We can get just use child.Value to get the string in this case
				break;
			case ConfigElementType.Array:
				// We can loop over this child's elements and print all the strings, say
				break;
			case ConfigElementType.Section:
				// We could loop over this child's elements
				break;
		}
	}

	// If we need to get at the root section, we can do that too
	ConfigSectionElement root = file.Root;
}
else
{
	Console.WriteLine("Failed to load file: " + result.ErrMsg);
}
```

## Examples
### Strings, Arrays, Comments
Here is a config file with some keys, strings, and string arrays, and descriptive comments.

```
# Syntax is simple
Key=Hello World!

# Keys can have spaces, and so can values. Quotes aren't even required!
Key With Spaces= Hello again, World!

# Strings can span multiple lines, but only if they are quoted. You can use "double quotes", 'single quotes', or `backticks` to quote strings
MultiLine="This is a multiline string
It spans many lines"

# Keys can have the = sign in them if you quote them
"Keys = Can be Quoted" = "And so can the values"

# Quotes work with both Keys and Values! You can use different quotes within strings
'Single"Quoted"Key'=`This string has backticks, so we don't get screwed by "different" quotes`

# Quotes only have an effect if they are the very first thing in the string. You can use all kinds of quotes all you want so long as they're not the first character of the string.
Doesn't Cause Problems=We're using "quotes" `just` fine!

# String Arrays are defined as per below.
"My Array"=[StringOne, String two, "String Three", "Multiline
within an array"]

# You don't need an equals sign when defining a array, it's optional (but may be easier to read). The below still works.
"Interpreted as an Array" [Element 1, Element 2]
```

### Sections
Sections are defined by {braces}. Using sections will cause the resultant paths of the config elements to be constructed the key of each section, and finally the key of the value. In the case of arrays, the key of the value is followed by the array index, to represent a specific string in the array.

```
global{
	# When loaded, this becomes global:ValueOne
	ValueOne = Hello Scope!

	nested{
		# You can nest sections, too. This will become global:nested:ValueTwo
		ValueTwo = Deeper Scope!
	}
	
	# Each of these strings gets a separate path. They are, in order: global:Collection:0, global:Array:1, global:Array:2
	Array=[ValueOne, ValueTwo, ValueThree]
}
```