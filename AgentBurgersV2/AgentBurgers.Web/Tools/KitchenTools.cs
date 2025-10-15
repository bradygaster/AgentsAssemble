using Microsoft.Extensions.AI;

namespace AgentBurgers.Web.Tools;

public static class GrillTools
{
    public static AITool CreateCookPattyTool(ILogger logger)
    {
        async Task<string> CookPatty(string pattyType, string doneness)
        {
            logger.LogInformation("🍔 GRILL: Starting to cook {PattyType} patty to {Doneness}", pattyType, doneness);
            await Task.Delay(Random.Shared.Next(500, 1000));
            var result = $"Cooked {pattyType} patty to {doneness} perfection";
            logger.LogInformation("🍔 GRILL: {Result}", result);
            return result;
        }

        return AIFunctionFactory.Create(CookPatty,
            "CookPatty",
            "Cook a burger patty to specified doneness. Parameters: pattyType (beef, turkey, veggie), doneness (rare, medium-rare, medium, well-done)");
    }

    public static AITool CreateMeltCheeseTool(ILogger logger)
    {
        async Task<string> MeltCheese(string cheeseType)
        {
            logger.LogInformation("🍔 GRILL: Melting {CheeseType} cheese", cheeseType);
            await Task.Delay(500);
            var result = $"Melted {cheeseType} cheese perfectly";
            logger.LogInformation("🍔 GRILL: {Result}", result);
            return result;
        }

        return AIFunctionFactory.Create(MeltCheese,
            "MeltCheese",
            "Melt cheese on a cooked patty. Parameters: cheeseType (cheddar, swiss, american)");
    }

    public static AITool CreateToastBunTool(ILogger logger)
    {
        async Task<string> ToastBun(string toastLevel)
        {
            logger.LogInformation("🍔 GRILL: Toasting buns to {ToastLevel}", toastLevel);
            await Task.Delay(500);
            var result = $"Toasted buns to {toastLevel} perfection";
            logger.LogInformation("🍔 GRILL: {Result}", result);
            return result;
        }

        return AIFunctionFactory.Create(ToastBun,
            "ToastBun",
            "Toast burger buns. Parameters: toastLevel (light, medium, dark)");
    }
}

public static class FryerTools
{
    public static AITool CreateFryFriesTool(ILogger logger)
    {
        async Task<string> FryFries(string friesType, string size)
        {
            logger.LogInformation("🍟 FRYER: Starting to fry {Size} {FriesType} fries", size, friesType);
            await Task.Delay(Random.Shared.Next(500, 1000));
            var result = $"Fried {size} {friesType} fries until golden and crispy";
            logger.LogInformation("🍟 FRYER: {Result}", result);
            return result;
        }

        return AIFunctionFactory.Create(FryFries,
            "FryFries",
            "Fry french fries. Parameters: friesType (regular, waffle, sweet-potato), size (small, medium, large)");
    }

    public static AITool CreateSeasonFriesTool(ILogger logger)
    {
        async Task<string> SeasonFries()
        {
            logger.LogInformation("🍟 FRYER: Seasoning fries");
            await Task.Delay(500);
            var result = "Seasoned fries with perfect salt and spice blend";
            logger.LogInformation("🍟 FRYER: {Result}", result);
            return result;
        }

        return AIFunctionFactory.Create(SeasonFries,
            "SeasonFries",
            "Season fries with salt and spices");
    }
}

public static class DessertTools
{
    public static AITool CreateMakeShakeTool(ILogger logger)
    {
        async Task<string> MakeShake(string flavor, string size)
        {
            logger.LogInformation("🥤 DESSERT: Making {Size} {Flavor} milkshake", size, flavor);
            await Task.Delay(Random.Shared.Next(2000, 3000));
            var result = $"Blended thick and creamy {size} {flavor} milkshake";
            logger.LogInformation("🥤 DESSERT: {Result}", result);
            return result;
        }

        return AIFunctionFactory.Create(MakeShake,
            "MakeShake",
            "Make a milkshake. Parameters: flavor (vanilla, chocolate, strawberry), size (small, medium, large)");
    }

    public static AITool CreateAddWhippedCreamTool(ILogger logger)
    {
        async Task<string> AddWhippedCream()
        {
            logger.LogInformation("🥤 DESSERT: Adding whipped cream");
            await Task.Delay(300);
            var result = "Added fresh whipped cream topping";
            logger.LogInformation("🥤 DESSERT: {Result}", result);
            return result;
        }

        return AIFunctionFactory.Create(AddWhippedCream,
            "AddWhippedCream",
            "Add whipped cream to desserts");
    }
}

public static class PlatingTools
{
    public static AITool CreateAssembleBurgerTool(ILogger logger)
    {
        async Task<string> AssembleBurger(string components)
        {
            logger.LogInformation("📦 PLATING: Assembling burger with: {Components}", components);
            await Task.Delay(500);
            var result = $"Assembled burger: {components}";
            logger.LogInformation("📦 PLATING: {Result}", result);
            return result;
        }

        return AIFunctionFactory.Create(AssembleBurger,
            "AssembleBurger",
            "Assemble burger with all components. Parameters: components (description of burger components)");
    }

    public static AITool CreatePackageMealTool(ILogger logger)
    {
        async Task<string> PackageMeal(string mealDescription)
        {
            logger.LogInformation("📦 PLATING: Packaging meal: {MealDescription}", mealDescription);
            await Task.Delay(250);
            var result = $"Packaged complete meal: {mealDescription}";
            logger.LogInformation("📦 PLATING: {Result}", result);
            return result;
        }

        return AIFunctionFactory.Create(PackageMeal,
            "PackageMeal",
            "Package meal for serving. Parameters: mealDescription (complete meal description)");
    }
}