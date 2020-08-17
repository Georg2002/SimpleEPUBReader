using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPUBParser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
	[TestClass]
	public class Toc
	{
		[TestMethod]
		public void TocTest1()
		{
			var TestCase = new TocTestCase()
			{
				FilePath = Path.Combine(TestResources.TestFolderPath, TestResources.TestEpubExtracted1, "OPS", "toc.ncx"),
				Chapters = 15,
				FirstChapterName = "とある魔術の禁書目録４",
				Title = "とある魔術の禁書目録４"
			};
			TocTester(TestCase);
		}

		[TestMethod]
		public void TocTest2()
		{
			var TestCase = new TocTestCase()
			{
				FilePath = Path.Combine(TestResources.TestFolderPath, TestResources.TestEpubExtracted2, "toc.ncx"),
				Chapters = 14,
				FirstChapterName = "星界の断章 Ⅰ",
				Title = "[森岡浩之] 星界の断章1"
			};
			TocTester(TestCase);
		}

		private void TocTester(TocTestCase testCase)
		{
			var TextFile = new ZipEntry() { Content = File.ReadAllBytes(testCase.FilePath) };
			TocInfo toc = new TocInfo(TextFile);
			Assert.IsTrue(toc.Title == testCase.Title);
			Assert.IsTrue(toc.Chapters.Count == testCase.Chapters);
			Assert.IsTrue(toc.Chapters[0] == testCase.FirstChapterName);
		}

		private class TocTestCase
        {
			public string FilePath;
			public int Chapters;
			public string FirstChapterName;
			public string Title;
		}
	}
}
