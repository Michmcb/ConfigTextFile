# ConfigTextFile
A .NET library which allows you to load plain-text files, which can be used as configuration files.

## Examples
### Strings, Collections, Comments
Here is a config file with some keys, strings, and string collections, and descriptive comments.

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

# String Collections are defined as per below. Be careful to not use an equals sign; that will cause it to be interpreted as a string!
"My Array" [StringOne, String two, "String Three", "Multiline
within an array"]

"Interpreted as a string"=[Not Actually, An Array]
```

### Scope
Scopes are defined by {braces}. Using scopes will give the keys a prefix. This can be configured but by default, the separator is a colon.

```
global{
	# When loaded, this becomes global:ValueOne
	ValueOne = Hello Scope!

	nested{
		# You can nest scope, too. This will become global:nested:ValueTwo
		ValueTwo = Deeper Scope!
	}
}
```