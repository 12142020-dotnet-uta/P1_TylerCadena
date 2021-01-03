using System.Collections.Generic;
using System.Linq;

using Microsoft.EntityFrameworkCore;

using TylerMart.Domain.Models;
using TylerMart.Storage.Contexts;

namespace TylerMart.Storage.Repositories {
	/// <summary>
	/// <see cref="TylerMart.Domain.Models.Location"/> Repository
	/// </summary>
	public class LocationRepository : Repository<Location> {
		/// <summary>
		/// Constructor that takes an instance of DatabaseContext
		/// </summary>
		/// <param name="db">Instance of DatabaseContext</param>
		public LocationRepository(DatabaseContext db) : base(db) {}
		/// <summary>
		/// Gets Location from name
		/// </summary>
		/// <param name="name">Name</param>
		/// <returns>
		/// Single Location or null
		/// </returns>
		public Location GetByName(string name) {
			return Db.Locations
				.SingleOrDefault(l => l.Name == name);
		}
		/// <summary>
		/// Finds Locations where a Product is in stock
		/// </summary>
		/// <param name="product">The Product</param>
		/// <returns>
		/// List of Locations
		/// </returns>
		public List<Location> FindFromProduct(Product product) {
			return Db.Set<LocationProduct>()
				.Where(lp => lp.ProductID == product.ID)
				.Include(lp => lp.Location)
				.Select(lp => lp.Location)
				.Distinct()
				.ToList();
		}
		/// <summary>
		/// Adds a Product to a Location's inventory
		/// </summary>
		/// <param name="location">The Location</param>
		/// <param name="product">The Product</param>
		/// <returns>
		/// 'true' if successfully inserted into database
		/// </returns>
		public bool AddProduct(Location location, Product product) {
			LocationProduct lp = new LocationProduct() {
				LocationID = location.ID,
				ProductID = product.ID
			};
			Db.Set<LocationProduct>().Add(lp);
			return base.Commit();
		}
		/// <summary>
		/// Removes a Product from a Location's inventory
		/// </summary>
		/// <param name="location">The Location</param>
		/// <param name="product">The Product</param>
		/// <returns>
		/// 'true' if successfully removed from database
		/// </returns>
		public bool RemoveProduct(Location location, Product product) {
			LocationProduct result = Db.Set<LocationProduct>()
				.FirstOrDefault(lp =>
					lp.LocationID == location.ID &&
					lp.ProductID == product.ID
				);
			if (result != null) {
				Db.Set<LocationProduct>().Remove(result);
				return base.Commit();
			}
			return false;
		}
		/// <summary>
		/// Adds a range of Products to a Location's inventory
		/// </summary>
		/// <param name="location">The Location</param>
		/// <param name="products">List of Products</param>
		/// <returns>
		/// 'true' if successfully inserted into database
		/// </returns>
		public bool AddProducts(Location location, List<Product> products) {
			List<LocationProduct> range = products.ConvertAll(product =>
				new LocationProduct() {
					LocationID = location.ID,
					ProductID = product.ID
				}
			);
			Db.Set<LocationProduct>().AddRange(range);
			return base.Commit();
		}
		/// <summary>
		/// Removes a range of Products from a Location's inventory
		/// </summary>
		/// <param name="location">The Location</param>
		/// <param name="products">List of Products</param>
		/// <returns>
		/// 'true' if successfully removed from database
		/// </returns>
		public bool RemoveProducts(Location location, List<Product> products) {
			List<LocationProduct> range = new List<LocationProduct>();
			products.ForEach(product => {
				LocationProduct result = Db.Set<LocationProduct>()
					.FirstOrDefault(lp =>
						lp.LocationID == location.ID &&
						lp.ProductID == product.ID &&
						!range.Contains(lp)
					);
				if (result != null) {
					range.Add(result);
				}
			});
			if (range.Count > 0) {
				Db.Set<LocationProduct>().RemoveRange(range);
				return base.Commit();
			}
			return false;
		}
	}
}
