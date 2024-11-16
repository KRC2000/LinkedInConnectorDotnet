using System.Text;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using System.Text.RegularExpressions;

namespace LinkedInConnectorDotnet;

class Program
{
	static async Task<int> Main(string[] args)
	{

		(string login, string pass) = GetCreds();

		InstallBrowser("webkit");

		var p = await Playwright.CreateAsync();
		var browser = await p.Webkit.LaunchAsync(new() { Headless = false });

		var page = await browser.NewPageAsync();
		await page.GotoAsync("https://www.linkedin.com/login");

		await page.FillAsync("#username", login);
		await page.FillAsync("#password", pass);

		await page.GetByRole(AriaRole.Button, new() { NameRegex = new("^sign in$", RegexOptions.IgnoreCase ) }).ClickAsync();

		await page.Locator("span").GetByText("my network", new() { Exact = false }).ClickAsync();

		Thread.Sleep(3000);

		for (int i = 0; i < 5; i++) {
			var loadMoreBtns = await page.GetByRole(AriaRole.Button).GetByText("Load more", new() { Exact = false }).AllAsync();
			await loadMoreBtns[0].ClickAsync();
			Console.WriteLine("Load more pressed");
			Thread.Sleep(5000);
		}

		var btns = await page.GetByRole(AriaRole.Button).GetByText("Connect").AllAsync();
		Console.WriteLine($"There is {btns.Count} buttons.");
		foreach (var b in btns) {
			Console.WriteLine(await b.TextContentAsync());
		}


		foreach (var b in btns) {
			await b.ClickAsync();
			Console.WriteLine("Connect button pressed");
			Thread.Sleep(1000);
		}


		Thread.Sleep(10000);
		return 0;
	}

	static void InstallBrowser(string browser)
	{
		var exitCode = Microsoft.Playwright.Program.Main([$"install", browser]);
		if (exitCode != 0)
			throw new Exception($"Playwright exited with code {exitCode}");
	}

	static (string, string) GetCreds() {
		StreamReader r = new("creds");
		string? login = r.ReadLine() ?? throw new Exception("Could not read login from the file.");
		string? pass = r.ReadLine() ?? throw new Exception("Could not read password from the file.");
		return (login, pass);
	}
}
