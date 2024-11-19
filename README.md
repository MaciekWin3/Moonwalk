<p align="center">
    <img src="./docs/logo.png" width=60%>
</p>

<h1 align="center"> 🌕 MoonWalk Compiler 🌕 </h1>

My first attempt at creating my own programming language.

## Description 📝
Moonwalk is designed to be a simple, safe, statically typed, compiled language fully compatible with the [.NET](https://dotnet.microsoft.com/en-us/) platform. It is designed to be a simple language that is easy to learn and use, while still being powerful enough to write complex programs in.

## Goals 🎯
- [ ] Simple impertaive language
- [ ] Full compatibility with .NET
- [ ] Safe, statically typed, compiled language
- [ ] Modular and eas y to write and create POC
- [ ] Inline testing

## Code Examples 📝
For now the syntax is still in flux, but here is a simple example of what I am aiming for:
```
import Terminal.Gui;

module Main {
	pub fn Main(args: string[]): void {
		Application.Init();

		let label: Label = create_label("Hello, World!");

		Application.Top.Add(label)
		Application.Run();
		Application.Shutdown();
	}

	prv fn CreateLabel(text: string): Label {
		let label = new Label(text) {
			X = Pos.Center(),
			Y = Pos.Center(),
			Height = 1
		};
		return label;
	}

	prv fn ImportExample(): void {
		Utils.Print("Hello, World");
	}
}

module Utils {

	pub struct Person() {

		Name: string;
		Age: int;

		init (name: string, age: int): Person {
			this.Name = name;
			if (age < 18) {
				Console.WriteLine("Younger person");
			}
			this.Age = age;
		}
	}

	pub fn Print(text: string): void {
		Console.WriteLine(string);
	}
}

module Main.Tests {

	import NUnit;

	[Test]
	pub fn ShouldAddTwoNumbers(): void {
		let result = 1 + 1;
		Assert.AreEqual(2, result);
	}
}
```

## References 📒

- Building a Compiler Series by Immo Landwerth: https://www.youtube.com/watch?v=wgHIkdUQbp0&list=PLRAdsfhKI4OWNOSfS7EUu5GRAVmze1t2y
- Introduction to Compilers and Language Design by Douglas Thain: https://www3.nd.edu/~dthain/compilerbook/
- Crafting a Compiler by Charles N. Fischer, Richard Joseph LeBlanc and Ronald Kaplan Cytron: https://www.amazon.com/Crafting-Compiler-Charles-N-Fischer/dp/0136067050