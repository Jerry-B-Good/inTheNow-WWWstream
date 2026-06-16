# Deliverables
For this week, you will be revisiting the application you wrote last week where you created a console application that accessed a third party web API.

You have the option of reusing the code from last week's work OR creating a new C# console application that consumes a web API. It should meet the following criteria:

+ Use the HttpClient class along with async and await to call a web API to gather data.
+ Deserialize that data - this time:
+ + Use List<T> to manage arrays of data returned by the API
+ + Use DateTime when deserializing date and time values returned by the API
+ + Where applicable, if constants are being returned by the API, deal with them as Enums as opposed to just string or integer values.
+ If your web API does not return DateTime or Enum values, that's OK - just make sure to use them in your application.

As always, feel free to get creative. Don't forget to create a Git repository and push your work to GitHub.
