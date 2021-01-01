﻿#pragma warning disable 1998

using System;
using System.Linq;
using System.Threading.Tasks;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using MudBlazor.UnitTests.Mocks;
using MudBlazor.UnitTests.TestComponents.Table;
using NUnit.Framework;


namespace MudBlazor.UnitTests
{

    [TestFixture]
    public class TableTests
    {
        private Bunit.TestContext ctx;

        [SetUp]
        public void Setup()
        {
            ctx = new Bunit.TestContext();
            ctx.JSInterop.Mode = JSRuntimeMode.Loose;
            ctx.Services.AddSingleton<NavigationManager>(new MockNavigationManager());
            ctx.Services.AddSingleton<IDialogService>(new DialogService());
            ctx.Services.AddSingleton<ISnackbar>(new MockSnackbar());
            ctx.Services.AddSingleton<IResizeListenerService>(new MockResizeListenerService());
        }

        [TearDown]
        public void TearDown() => ctx.Dispose();
        
        /// <summary>
        /// OnRowClick event callback should be fired regardless of the selection state
        /// </summary>
        [Test]
        public void TableRowClick()
        {
            var comp = ctx.RenderComponent<TableRowClickTest>();
            Console.WriteLine(comp.Markup);
            comp.Find("p").TextContent.Trim().Should().BeEmpty();
            var trs = comp.FindAll("tr");
            trs[1].Click();
            comp.Find("p").TextContent.Trim().Should().Be("0");
            trs[1].Click();
            comp.Find("p").TextContent.Trim().Should().Be("0,0");
            trs[2].Click();
            comp.Find("p").TextContent.Trim().Should().Be("0,0,1");
            trs[0].Click(); // clicking the header row shouldn't to anything
            comp.Find("p").TextContent.Trim().Should().Be("0,0,1");
        }

        [Test]
        [Ignore("todo")]
        public void TableSingleSelection()
        {
            //var comp = ctx.RenderComponent<TableSingleSelection>();
            // print the generated html
            //Console.WriteLine(comp.Markup);
            // select elements needed for the test
            //var group = comp.FindComponent<MudTable>();
            // ...

            // the item of the clicked row should be in SelectedItem
        }

        [Test]
        [Ignore("todo")]
        public void TableFilter()
        {
            // non-matching rows should disappear
        }

        [Test]
        [Ignore("todo")]
        public void TablePaging()
        {
            // there must not be more rows than page size
            // switch page and check the rows of the selected page
        }

        /// <summary>
        /// the selected items (check-box click or row click) should be in SelectedItems
        /// </summary>
        [Test]
        public void TableMultiSelectionTest1()
        {
            var comp = ctx.RenderComponent<TableMultiSelectionTest1>();
            // print the generated html
            Console.WriteLine(comp.Markup);
            // select elements needed for the test
            var table = comp.FindComponent<MudTable<int>>().Instance;
            var text = comp.FindComponent<MudText>();
            var tr = comp.FindAll("tr").ToArray();
            tr.Length.Should().Be(3);
            var td = comp.FindAll("td").ToArray();
            td.Length.Should().Be(6); // two td per row for multi selection
            var inputs = comp.FindAll("input").ToArray();
            inputs.Length.Should().Be(3); // one checkbox per row
            table.SelectedItems.Count.Should().Be(0); // selected items should be empty
            // click checkboxes and verify selection text
            inputs[0].Change(true);
            table.SelectedItems.Count.Should().Be(1);
            comp.Find("p").TextContent.Should().Be("SelectedItems { 0 }");
            inputs = comp.FindAll("input").ToArray();
            inputs[0].Change(false);
            table.SelectedItems.Count.Should().Be(0);
            comp.Find("p").TextContent.Should().Be("SelectedItems {  }");
            // row click
            tr[1].Click();
            comp.Find("p").TextContent.Should().Be("SelectedItems { 1 }");
        }

        /// <summary>
        /// checking the header checkbox should select all items (all checkboxes on, all items in SelectedItems)
        /// </summary>
        [Test]
        public void TableMultiSelectionTest2()
        {
            var comp = ctx.RenderComponent<TableMultiSelectionTest2>();
            // print the generated html
            Console.WriteLine(comp.Markup);
            // select elements needed for the test
            var table = comp.FindComponent<MudTable<int>>().Instance;
            var text = comp.FindComponent<MudText>();
            var checkboxes = comp.FindComponents<MudCheckBox<bool>>().Select(x=>x.Instance).ToArray();
            var tr = comp.FindAll("tr").ToArray();
            tr.Length.Should().Be(4); // <-- one header, three rows
            var th = comp.FindAll("th").ToArray();
            th.Length.Should().Be(2); //  one for the checkbox, one for the header
            var td = comp.FindAll("td").ToArray();
            td.Length.Should().Be(6); // two td per row for multi selection
            var inputs = comp.FindAll("input").ToArray();
            inputs.Length.Should().Be(4); // one checkbox per row + one for the header
            table.SelectedItems.Count.Should().Be(0); // selected items should be empty
            // click header checkbox and verify selection text
            inputs[0].Change(true);
            table.SelectedItems.Count.Should().Be(3);
            comp.Find("p").TextContent.Should().Be("SelectedItems { 0, 1, 2 }");
            checkboxes.Sum(x => x.Checked ? 1 : 0).Should().Be(4);
            inputs = comp.FindAll("input").ToArray();
            inputs[0].Change(false);
            table.SelectedItems.Count.Should().Be(0);
            comp.Find("p").TextContent.Should().Be("SelectedItems {  }");
            checkboxes.Sum(x => x.Checked ? 1 : 0).Should().Be(0);
        }

        /// <summary>
        /// Initially the values bound to SelectedItems should be selected
        /// </summary>
        [Test]
        public void TableMultiSelectionTest3()
        {
            var comp = ctx.RenderComponent<TableMultiSelectionTest3>();
            // print the generated html
            Console.WriteLine(comp.Markup);
            // select elements needed for the test
            var table = comp.FindComponent<MudTable<int>>().Instance;
            var text = comp.FindComponent<MudText>();
            var checkboxes = comp.FindComponents<MudCheckBox<bool>>().Select(x => x.Instance).ToArray();
            table.SelectedItems.Count.Should().Be(1); // selected items should be empty
            comp.Find("p").TextContent.Should().Be("SelectedItems { 1 }");
            checkboxes.Sum(x => x.Checked ? 1 : 0).Should().Be(1);
            checkboxes[0].Checked.Should().Be(false);
            checkboxes[2].Checked.Should().Be(true);
            // uncheck it
            comp.InvokeAsync(() => { 
                checkboxes[2].Checked = false;
            });
            comp.WaitForState(()=>checkboxes[2].Checked==false);
            table.SelectedItems.Count.Should().Be(0);
            comp.Find("p").TextContent.Should().Be("SelectedItems {  }");
            checkboxes.Sum(x => x.Checked ? 1 : 0).Should().Be(0);
        }

        /// <summary>
        /// The checkboxes should all be checked on load, even the header checkbox.
        /// </summary>
        [Test]
        public void TableMultiSelectionTest4()
        {
            var comp = ctx.RenderComponent<TableMultiSelectionTest4>();
            // print the generated html
            Console.WriteLine(comp.Markup);
            // select elements needed for the test
            var table = comp.FindComponent<MudTable<int>>().Instance;
            var text = comp.FindComponent<MudText>();
            var checkboxes = comp.FindComponents<MudCheckBox<bool>>().Select(x => x.Instance).ToArray();
            table.SelectedItems.Count.Should().Be(3); 
            comp.Find("p").TextContent.Should().Be("SelectedItems { 0, 1, 2 }");
            checkboxes.Sum(x => x.Checked ? 1 : 0).Should().Be(4);
            // uncheck only row 1 => header checkbox should be off then
            comp.InvokeAsync(() => {
                checkboxes[2].Checked = false;
            });
            comp.WaitForState(() => checkboxes[2].Checked == false);
            checkboxes[0].Checked.Should().Be(false); // header checkbox should be off
            table.SelectedItems.Count.Should().Be(2);
            comp.Find("p").TextContent.Should().Be("SelectedItems { 0, 2 }");
            checkboxes.Sum(x => x.Checked ? 1 : 0).Should().Be(2);
        }

        /// <summary>
        /// Paging should not influence multi-selection
        /// </summary>
        [Test]
        public async Task TableMultiSelectionTest5()
        {
            ctx.Services.AddSingleton<NavigationManager>(new MockNavigationManager());
            var comp = ctx.RenderComponent<TableMultiSelectionTest5>();
            // print the generated html
            Console.WriteLine(comp.Markup);
            // select elements needed for the test
            var table = comp.FindComponent<MudTable<int>>().Instance;
            var text = comp.FindComponent<MudText>();
            var checkboxes = comp.FindComponents<MudCheckBox<bool>>().Select(x => x.Instance).ToArray();
            table.SelectedItems.Count.Should().Be(4); 
            comp.Find("p").TextContent.Should().Be("SelectedItems { 0, 1, 2, 3 }");
            checkboxes.Sum(x => x.Checked ? 1 : 0).Should().Be(3);
            // uncheck a row then switch to page 2 and both checkboxes on page 2 should be checked
            await comp.InvokeAsync(() => checkboxes[1].Checked = false);
            checkboxes.Sum(x => x.Checked ? 1 : 0).Should().Be(1);
            // switch page 
            await comp.InvokeAsync(() => table.CurrentPage = 1);
            // now two checkboxes should be checked on page 2
            checkboxes = comp.FindComponents<MudCheckBox<bool>>().Select(x => x.Instance).ToArray();
            checkboxes.Sum(x => x.Checked ? 1 : 0).Should().Be(2);
        }

        /// <summary>
        /// Setting items delayed should work well and update pager also
        /// </summary>
        [Test]
        public async Task TablePaginationTest1()
        {
            var comp = ctx.RenderComponent<TablePaginationTest1>();
            await Task.Delay(200);
            Console.WriteLine(comp.Markup);
            comp.FindAll("tr.mud-table-row").Count.Should().Be(11); // ten rows + header row
            comp.FindAll("p.mud-table-pagination-caption").Last().TextContent.Trim().Should().Be("1-10 of 20");
        }

        /// <summary>
        /// Even without a MudTablePager the table should call ServerReload to get the items on start.
        /// </summary>
        [Test]
        public async Task TableServerSideDataTest1()
        {
            var comp = ctx.RenderComponent<TableServerSideDataTest1>();
            Console.WriteLine(comp.Markup);
            comp.FindAll("tr").Count.Should().Be(4); // three rows + header row
            comp.FindAll("td")[0].TextContent.Trim().Should().Be("1");
            comp.FindAll("td")[1].TextContent.Trim().Should().Be("2");
            comp.FindAll("td")[2].TextContent.Trim().Should().Be("3");
        }

        /// <summary>
        /// The table should call ServerReload to get the items for the current page according to MudTablePager
        /// </summary>
        [Test]
        public async Task TableServerSideDataTest2()
        {
            var comp = ctx.RenderComponent<TableServerSideDataTest2>();
            Console.WriteLine(comp.Markup);
            comp.FindAll("tr").Count.Should().Be(4); // three rows + header row
            comp.FindAll("td")[0].TextContent.Trim().Should().Be("1");
            comp.FindAll("td")[1].TextContent.Trim().Should().Be("2");
            comp.FindAll("td")[2].TextContent.Trim().Should().Be("3");
            comp.FindAll("div.mud-table-pagination-actions button")[2].Click(); // next >
            comp.FindAll("td")[0].TextContent.Trim().Should().Be("4");
            comp.FindAll("td")[1].TextContent.Trim().Should().Be("5");
            comp.FindAll("td")[2].TextContent.Trim().Should().Be("6");
            comp.FindAll("div.mud-table-pagination-actions button")[2].Click(); // next >
            comp.FindAll("td")[0].TextContent.Trim().Should().Be("7");
            comp.FindAll("td")[1].TextContent.Trim().Should().Be("8");
            comp.FindAll("td")[2].TextContent.Trim().Should().Be("9");
            comp.FindAll("div.mud-table-pagination-actions button")[0].Click(); // |<
            comp.FindAll("td")[0].TextContent.Trim().Should().Be("1");
            comp.FindAll("td")[1].TextContent.Trim().Should().Be("2");
            comp.FindAll("td")[2].TextContent.Trim().Should().Be("3");
        }

    }
}
