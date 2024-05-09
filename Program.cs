using NLog;
using Northwind_Console.Model;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
// using NWConsole.Model;

// See https://aka.ms/new-console-template for more information
string path = Directory.GetCurrentDirectory() + "\\nlog.config";

// create instance of Logger
var logger = LogManager.LoadConfiguration(path).GetCurrentClassLogger();
logger.Info("Program started");

try
{
    var db = new NWContext();
    string choice;
    do
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("1) Display Categories");
        Console.WriteLine("2) Add Category");
        Console.WriteLine("3) Display Category and related products");
        Console.WriteLine("4) Display all Categories and their related products");
        Console.WriteLine("5) Add Product");
        Console.WriteLine("6) Edit Product");
        Console.WriteLine("\"q\" to quit");
        choice = Console.ReadLine();
        Console.Clear();
        logger.Info($"Option {choice} selected");
        if (choice == "1")
        {
            var query = db.Categories.OrderBy(p => p.CategoryName);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{query.Count()} records returned");
            Console.ForegroundColor = ConsoleColor.Magenta;
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryName} - {item.Description}");
            }
            Console.ForegroundColor = ConsoleColor.White;

        }
        else if (choice == "2")
        {
            Category category = new Category();
            Console.WriteLine("Enter Category Name:");
            category.CategoryName = Console.ReadLine();
            Console.WriteLine("Enter the Category Description:");
            category.Description = Console.ReadLine();
            // TODO: save category to db
            ValidationContext context = new ValidationContext(category, null, null);
            List<ValidationResult> results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(category, context, results, true);
            if (isValid)
            {
                // check for unique name
                if (db.Categories.Any(c => c.CategoryName == category.CategoryName))
                {
                    // generate validation error
                    isValid = false;
                    results.Add(new ValidationResult("Name exists", new string[] { "CategoryName" }));
                }
                else
                {
                    logger.Info("Validation passed");
                    // TODO: save category to db
                }
            }
            if (!isValid)
            {
                foreach (var result in results)
                {
                    logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                }
            }
            if (category != null)
            {

                db.addCategories(category);
                logger.Info($"Category added - (Name) {category.CategoryName}");
            }
        }
        else if (choice == "3")
        {
            var query = db.Categories.OrderBy(p => p.CategoryId);

            Console.WriteLine("Select the category whose products you want to display:");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
            }
            Console.ForegroundColor = ConsoleColor.White;
            int id = int.Parse(Console.ReadLine());
            Console.Clear();
            logger.Info($"CategoryId {id} selected");
            Category category = db.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == id);
            Console.WriteLine($"{category.CategoryName} - {category.Description}");
            foreach (Product p in category.Products)
            {
                Console.WriteLine($"\t{p.ProductName}"); ;
            }
        }
        else if (choice == "4")
        {
            var query = db.Categories.Include("Products").OrderBy(p => p.CategoryId);
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryName}");
                foreach (Product p in item.Products)
                {
                    Console.WriteLine($"\t{p.ProductName}");
                }
            }
        }
        // Console.WriteLine();
        else if (choice == "5")
        {
            Product product = new Product();
            Supplier supplier = new Supplier();
            Category category = new Category();
            Console.WriteLine("Enter Product Name");
            string Name = Console.ReadLine();

            var query = db.Suppliers.OrderBy(p => p.SupplierId);
            Console.WriteLine("Select a Suplier ID");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            foreach (var item in query)
            {

                Console.WriteLine($"{item.SupplierId}) - {item.CompanyName}");
            }
            Console.ForegroundColor = ConsoleColor.White;
            int id = int.Parse(Console.ReadLine());
            Console.Clear();
            logger.Info($"SupplierId {id} selected");
            supplier.SupplierId = id;

            var query1 = db.Categories.OrderBy(c => c.CategoryId);
            Console.WriteLine("Select Category ID");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            foreach (var item in query1)
            {

                Console.WriteLine($"{item.CategoryId} - {item.CategoryName}");
            }
            Console.ForegroundColor = ConsoleColor.White;
            int IDc = int.Parse(Console.ReadLine());
            Console.Clear();
            logger.Info($"CategoryId {IDc} selected");
            category.CategoryId = IDc;

            Console.WriteLine("Enter QTY of Product");
            string QTY = Console.ReadLine();

            Console.WriteLine("Enter Unit Pirce");
            string UnitPrice = product.UnitPrice.ToString();

            UnitPrice = Console.ReadLine();
            var UnitDec = decimal.Parse(UnitPrice);


            Console.WriteLine("Enter Units in Stock");
            string Stock = product.UnitsInStock.ToString();
            Stock = Console.ReadLine();
            var convertStock = short.Parse(Stock);

            Console.WriteLine("Enter Units on Order");
            string Order = product.UnitsOnOrder.ToString();
            Order = Console.ReadLine();
            var convertOrder = short.Parse(Order);

            Console.WriteLine("Enter Recored Level");
            string records = product.ReorderLevel.ToString();
            records = Console.ReadLine();
            var convertRecords = short.Parse(records);

            Console.WriteLine("Enter Discontinued Products");
            Console.WriteLine("(Discontinued = True ) (Not Discontinued = False)");
            string discontinued = product.Discontinued.ToString();
            discontinued = Console.ReadLine();
            var convertDiscontnued = bool.Parse(discontinued);
            // if(discontinued == "1"){
            //     convertDiscontnued = false;
            // }else if(discontinued =="0"){
            //     convertDiscontnued = true;
            // }

            var ProductAdd = new Product { ProductName = Name, SupplierId = id, CategoryId = IDc, QuantityPerUnit = QTY, UnitPrice = UnitDec, UnitsInStock = convertStock, UnitsOnOrder = convertOrder, ReorderLevel = convertRecords};

            db.addProducts(ProductAdd);

        }
        else if(choice == "6"){
           
           Console.WriteLine("Choose witch product to edit:");
           var product = GetProduct(db, logger);
           if(product != null){

            Product updateProduct = InputProduct(db, logger);
            if (updateProduct != null){

                updateProduct.ProductId = product.ProductId;
                db.editProducts(updateProduct);
                 logger.Info($"Product (id: {product.ProductId}) updated");
            }
           }

        }

    } while (choice.ToLower() != "q");
}
catch (Exception ex)
{
    logger.Error(ex.Message);
}

logger.Info("Program ended");


static Product GetProduct(NWContext db, Logger logger){

    var product = db.Products.OrderBy(p => p.ProductId);
    foreach(Product p in product){

        Console.WriteLine($"{p.ProductId}: {p.ProductName}");
    }
    if (int.TryParse(Console.ReadLine(), out int ProductId))
    {
        Product product1 = db.Products.FirstOrDefault(p => p.ProductId == ProductId);
        if (product1 != null)
        {
            return product1;
        }
        logger.Error("Invalid Product Id");
    }
   return null;
}

static Product InputProduct(NWContext db, Logger logger){
      
      Product product = new Product();
      Console.WriteLine("Enter new product name:");
      product.ProductName = Console.ReadLine();
       ValidationContext context = new ValidationContext(product, null, null);
    List<ValidationResult> results = new List<ValidationResult>();

    var isValid = Validator.TryValidateObject(product, context, results, true);
    if (isValid)
    {
        return product;
    }
    else
    {
        foreach (var result in results)
        {
            logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
        }
    }
    return null;


}