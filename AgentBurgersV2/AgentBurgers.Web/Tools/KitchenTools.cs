using System.ComponentModel;
using Microsoft.Extensions.AI;

namespace AgentBurgers.Web.Tools;

public static class GrillTools
{
    [Description("Cook a burger patty to specified doneness")]
    public static async Task<string> CookPatty(
        [Description("Type of patty: beef, turkey, veggie")] string pattyType,
        [Description("Doneness: rare, medium-rare, medium, well-done")] string doneness)
    {
        await Task.Delay(Random.Shared.Next(2000, 4000));
        return $"Cooked {pattyType} patty to {doneness} perfection";
    }

    [Description("Melt cheese on a cooked patty")]
    public static async Task<string> MeltCheese(
        [Description("Cheese type: cheddar, swiss, american")] string cheeseType)
    {
        await Task.Delay(1500);
        return $"Melted {cheeseType} cheese perfectly";
    }

    [Description("Toast burger buns")]
    public static async Task<string> ToastBun(
        [Description("Toast level: light, medium, dark")] string toastLevel)
    {
        await Task.Delay(1000);
        return $"Toasted buns to {toastLevel} perfection";
    }
}

public static class FryerTools
{
    [Description("Fry french fries")]
    public static async Task<string> FryFries(
        [Description("Fries type: regular, waffle, sweet-potato")] string friesType,
        [Description("Size: small, medium, large")] string size)
    {
        await Task.Delay(Random.Shared.Next(3000, 5000));
        return $"Fried {size} {friesType} fries until golden and crispy";
    }

    [Description("Season fries with salt and spices")]
    public static async Task<string> SeasonFries()
    {
        await Task.Delay(500);
        return "Seasoned fries with perfect salt and spice blend";
    }
}

public static class DessertTools
{
    [Description("Make a milkshake")]
    public static async Task<string> MakeShake(
        [Description("Flavor: vanilla, chocolate, strawberry")] string flavor,
        [Description("Size: small, medium, large")] string size)
    {
        await Task.Delay(Random.Shared.Next(2000, 3000));
        return $"Blended thick and creamy {size} {flavor} milkshake";
    }

    [Description("Add whipped cream to desserts")]
    public static async Task<string> AddWhippedCream()
    {
        await Task.Delay(300);
        return "Added fresh whipped cream topping";
    }
}

public static class PlatingTools
{
    [Description("Assemble burger with all components")]
    public static async Task<string> AssembleBurger(
        [Description("Description of burger components")] string components)
    {
        await Task.Delay(1500);
        return $"Assembled burger: {components}";
    }

    [Description("Package meal for serving")]
    public static async Task<string> PackageMeal(
        [Description("Complete meal description")] string mealDescription)
    {
        await Task.Delay(1000);
        return $"Packaged complete meal: {mealDescription}";
    }
}