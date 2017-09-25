public interface IStoreProduct
{

	string Name { get; set; }

	string Description { get; set; }

	string DetailedDescription { get; set; }

	string PrefabId { get; set; }

	bool IsVisible { get; set; }

}