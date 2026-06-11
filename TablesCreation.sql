CREATE DATABASE ShoppDB;

USE ShoppDB;


BEGIN TRANSACTION;
BEGIN TRY

CREATE TABLE Users(
	UserId INT IDENTITY(1,1) PRIMARY KEY,
	Name NVARCHAR(100) NOT NULL,
	Email NVARCHAR(100) NOT NULL UNIQUE,
	PasswordHash NVARCHAR(255) NOT NULL,
	MobileNo VARCHAR(15) UNIQUE,
	Address NVARCHAR(500),
	Role VARCHAR(20) NOT NULL CHECK(Role IN ('User','Retailer','Admin')),
	CreatedDate DATETIME DEFAULT GETDATE()
);

CREATE TABLE Categories(
	CategoryId INT IDENTITY(1,1) PRIMARY KEY,
	CategoryName NVARCHAR(100) NOT NULL UNIQUE,
	Description NVARCHAR(500)
);

CREATE TABLE Brands(
	BrandId INT IDENTITY(1,1) PRIMARY KEY,
	BrandName NVARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE Products(
	ProductId INT IDENTITY(1,1) PRIMARY KEY,
	ProductName NVARCHAR(200) NOT NULL,
	Description NVARCHAR(MAX),
	Price DECIMAL(10,2) NOT NULL CHECK(Price>=0),
	Stock INT NOT NULL DEFAULT 0 CHECK(Stock>=0),
	CategoryId INT NOT NULL,
	BrandId INT NOT NULL,
	RetailerId INT NOT NULL,
	Status VARCHAR(20) NOT NULL DEFAULT 'Pending'
		CHECK(Status IN ('Pending','Approved','Rejected')),
	IsActive BIT NOT NULL DEFAULT 1,
	CreatedDate DATETIME DEFAULT GETDATE(),
		FOREIGN KEY(CategoryId) REFERENCES Categories(CategoryId),
		FOREIGN KEY(BrandId) REFERENCES Brands(BrandId),
		FOREIGN KEY(RetailerId) REFERENCES Users(UserId)
);

CREATE TABLE ProductImages(
	ImageId INT IDENTITY(1,1) PRIMARY KEY,
	ProductId INT NOT NULL,
	ImageURL NVARCHAR(500) NOT NULL,
	IsPrimary BIT DEFAULT 0,
	FOREIGN KEY(ProductId) REFERENCES Products(ProductId)
);

CREATE TABLE Wishlist(
	WishlistId INT IDENTITY(1,1) PRIMARY KEY,
	UserId INT NOT NULL,
	ProductId INT NOT NULL,
	AddedDate DATETIME DEFAULT GETDATE(),
	FOREIGN KEY(UserId) REFERENCES Users(UserId),
	FOREIGN KEY(ProductId) REFERENCES Products(ProductId),
	CONSTRAINT UQ_Wishlist UNIQUE(UserId,ProductId)
);

CREATE TABLE CompareProducts(
	CompareId INT IDENTITY(1,1) PRIMARY KEY,
	UserId INT NOT NULL,
	ProductId INT NOT NULL,
	AddedDate DATETIME DEFAULT GETDATE(),
	FOREIGN KEY(UserId) REFERENCES Users(UserId),
	FOREIGN KEY(ProductId) REFERENCES Products(ProductId),
	CONSTRAINT UQ_Compare UNIQUE(UserId,ProductId)
);

CREATE TABLE Cart(
	CartId INT IDENTITY(1,1) PRIMARY KEY,
	UserId INT NOT NULL UNIQUE,
	CreatedDate DATETIME DEFAULT GETDATE(),
	FOREIGN KEY(UserId) REFERENCES Users(UserId)
);

CREATE TABLE CartItems(
	CartItemId INT IDENTITY(1,1) PRIMARY KEY,
	CartId INT NOT NULL,
	ProductId INT NOT NULL,
	Quantity INT NOT NULL CHECK(Quantity>0),
	UnitPrice DECIMAL(10,2) NOT NULL,
	FOREIGN KEY(CartId) REFERENCES Cart(CartId),
	FOREIGN KEY(ProductId) REFERENCES Products(ProductId)
);

CREATE TABLE Orders(
	OrderId INT IDENTITY(1,1) PRIMARY KEY,
	UserId INT NOT NULL,
	OrderDate DATETIME DEFAULT GETDATE(),
	TotalAmount DECIMAL(12,2) NOT NULL,
	OrderStatus VARCHAR(20) NOT NULL DEFAULT 'Pending'
		CHECK(OrderStatus IN ('Pending','Confirmed','Shipped','Delivered','Cancelled')),
		FOREIGN KEY(UserId) REFERENCES Users(UserId)
);

CREATE TABLE OrderDetails(
	OrderDetailId INT IDENTITY(1,1) PRIMARY KEY,
	OrderId INT NOT NULL,
	ProductId INT NOT NULL,
	Quantity INT NOT NULL CHECK(Quantity>0),
	Price DECIMAL(10,2) NOT NULL,
	FOREIGN KEY(OrderId) REFERENCES Orders(OrderId),
	FOREIGN KEY(ProductId) REFERENCES Products(ProductId)
);

CREATE TABLE OTPs(
	OtpId INT IDENTITY(1,1) PRIMARY KEY,
	UserId INT NOT NULL,
	OtpCode VARCHAR(10) NOT NULL,
	ExpiryTime DATETIME NOT NULL,
	IsUsed BIT DEFAULT 0,
	FOREIGN KEY(UserId) REFERENCES Users(UserId)
);

CREATE TABLE ProductApproval(
	ApprovalId INT IDENTITY(1,1) PRIMARY KEY,
	ProductId INT NOT NULL,
	RetailerId INT NOT NULL,
	AdminId INT NOT NULL,
	ApprovalStatus VARCHAR(20) NOT NULL CHECK(ApprovalStatus IN ('Approved','Rejected')),
	Remarks NVARCHAR(500),
	ApprovalDate DATETIME DEFAULT GETDATE(),
		FOREIGN KEY(ProductId) REFERENCES Products(ProductId),
		FOREIGN KEY(RetailerId) REFERENCES Users(UserId),
		FOREIGN KEY(AdminId) REFERENCES Users(UserId)
);

CREATE TABLE Reviews(
	ReviewId INT IDENTITY(1,1) PRIMARY KEY,
	UserId INT NOT NULL,
	ProductId INT NOT NULL,
	Rating INT NOT NULL CHECK(Rating BETWEEN 1 AND 5),
	ReviewText NVARCHAR(1000),
	ReviewDate DATETIME DEFAULT GETDATE(),
		FOREIGN KEY(UserId) REFERENCES Users(UserId),
		FOREIGN KEY(ProductId) REFERENCES Products(ProductId),
		CONSTRAINT UQ_Review UNIQUE(UserId,ProductId)
);

CREATE TABLE Payments(
	PaymentId INT IDENTITY(1,1) PRIMARY KEY,
	OrderId INT NOT NULL,
	Amount DECIMAL(12,2) NOT NULL,
	PaymentDate DATETIME DEFAULT GETDATE(),
	PaymentMethod VARCHAR(20),
	PaymentStatus VARCHAR(20),
	TransactionId VARCHAR(100) UNIQUE,
	FOREIGN KEY(OrderId) REFERENCES Orders(OrderId)
);

COMMIT TRANSACTION;
PRINT 'Database created successfully';

END TRY
BEGIN CATCH
 ROLLBACK TRANSACTION;
 THROW;
END CATCH;

SELECT * FROM Products
SELECT * FROM Categories;
SELECT * FROM Brands;
SELECT * FROM Users;

INSERT INTO Categories (CategoryName, Description)
VALUES
('Laptop', 'Laptop products'),
('Mobile', 'Mobile phones'),
('Accessories', 'Electronic accessories');

INSERT INTO Brands (BrandName)
VALUES
('Apple'),
('Samsung'),
('Dell'),
('HP');

INSERT INTO Products
(
    ProductName,
    Description,
    Price,
    Stock,
    CategoryId,
    BrandId,
    RetailerId,
    Status,
    IsActive,
    CreatedDate
)
VALUES
(
    'MacBook M2 Pro',
    'Apple laptop',
    89000,
    10,
    1,
    1,
    1,
    'Approved',
    1,
    GETDATE()
);

SELECT TOP 1 * FROM Users;

INSERT INTO Users
(
    Name,
    Email,
    PasswordHash,
    MobileNo,
    Address,
    Role,
    CreatedDate
)
VALUES
(
    'Admin User',
    'admin@shopsphere.com',
    '123456',
    '9876543210',
    'Chennai',
    'Retailer',
    GETDATE()
);

SELECT * FROM Users;

SELECT * FROM Categories;
SELECT * FROM Brands;