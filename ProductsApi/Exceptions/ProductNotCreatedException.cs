namespace ProductsApi.Exceptions;

public class ProductNotCreatedException(string name) : BaseException($"product {name} could not be created");
