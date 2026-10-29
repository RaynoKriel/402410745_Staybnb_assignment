-- =====================================================================================
-- ADDITIONAL DATA LOAD
-- Adds extra sample data (users, Cape Town properties, bookings, reviews, messages,
-- notifications and activity logs) so the application does not look empty.
--
-- HOW TO RUN
--   1. Start the website once (Run_Staybnb.bat) so the database and tables exist. Then you
--      can close it.
--   2. Open SQL Server Management Studio (or Visual Studio > SQL Server Object Explorer),
--      connect to  (localdb)\MSSQLLocalDB , open this file and click Execute (F5).
--      If your database is not called StaybnbDb, change the name on the USE line below.
--   3. Start the website again. Refresh the pages.
--
-- DEMO LOGINS (password for all of them:  Demo@123)
--   Admin   royal@staybnb.com
--   Hosts   gideon@staybnb.com   marco@staybnb.com   brandon@staybnb.com
--   Guests  reino@staybnb.com    john@staybnb.com    courtney@staybnb.com
--
-- PROPERTY PHOTOS: the script points to files in the website folder  wwwroot\uploads
-- It is safe to run only once; a second run does nothing.
-- =====================================================================================
USE [402410745StaybnbDb];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID(N'dbo.AspNetUsers') IS NULL OR OBJECT_ID(N'dbo.HostProperties') IS NULL
BEGIN
    RAISERROR(N'Tables not found. Start the website once first, and make sure 402410745StaybnbDb is selected.', 16, 1);
    RETURN;
END

IF EXISTS (SELECT 1 FROM dbo.AspNetUsers WHERE Email = N'gideon@staybnb.com')
BEGIN
    PRINT N'The additional data was already loaded. Nothing to do.';
    RETURN;
END

IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE Name = N'Host')
BEGIN
    RAISERROR(N'Roles not found. Start the website once first so the roles are created.', 16, 1);
    RETURN;
END

BEGIN TRANSACTION;

-- =====================================================================================
-- 1. USERS (AspNetUsers) - password for every user is Demo@123    this is for demo use
-- =====================================================================================
INSERT INTO dbo.AspNetUsers
    (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash,
     SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled,
     AccessFailedCount, FirstName, LastName, CreatedAt)
VALUES
    (N'demo-host-gideon', N'gideon@staybnb.com', N'GIDEON@STAYBNB.COM', N'gideon@staybnb.com', N'GIDEON@STAYBNB.COM', 1, N'AQAAAAIAAYagAAAAEHGtCjMsmjlYXa7Gc/US8zXMoTbMiIRs17YDdotszZ1c5xzRMpGXGDlFLwEVcYAuoA==',
     CONVERT(nvarchar(36), NEWID()), CONVERT(nvarchar(36), NEWID()), 0, 0, 1, 0, N'Gideon', N'Pieterse', DATEADD(DAY, -60, GETDATE())),
    (N'demo-host-marco', N'marco@staybnb.com', N'MARCO@STAYBNB.COM', N'marco@staybnb.com', N'MARCO@STAYBNB.COM', 1, N'AQAAAAIAAYagAAAAELEyHHkoIlLn1V6Xk561qwdcAdl7AMKkcHlCSEbisRrnwBftyQLgINwnG930kP89YQ==',
     CONVERT(nvarchar(36), NEWID()), CONVERT(nvarchar(36), NEWID()), 0, 0, 1, 0, N'Marco', N'Jacobs', DATEADD(DAY, -58, GETDATE())),
    (N'demo-host-brandon', N'brandon@staybnb.com', N'BRANDON@STAYBNB.COM', N'brandon@staybnb.com', N'BRANDON@STAYBNB.COM', 1, N'AQAAAAIAAYagAAAAEGpjltzuCPigtnMEnb8BMaC1jY3h9EvtpvfmCz7XkYkRuPs2xmZy/ehbU2bHCpDUEg==',
     CONVERT(nvarchar(36), NEWID()), CONVERT(nvarchar(36), NEWID()), 0, 0, 1, 0, N'Brandon', N'Adams', DATEADD(DAY, -55, GETDATE())),
    (N'demo-guest-reino', N'reino@staybnb.com', N'REINO@STAYBNB.COM', N'reino@staybnb.com', N'REINO@STAYBNB.COM', 1, N'AQAAAAIAAYagAAAAEIK69FVv5dJUWdRa1R5YvBHgtCQxKtyiZXacW05BK4GqtH+qO6gdrXlRlo8cTzybDw==',
     CONVERT(nvarchar(36), NEWID()), CONVERT(nvarchar(36), NEWID()), 0, 0, 1, 0, N'Reino', N'Creel', DATEADD(DAY, -45, GETDATE())),
    (N'demo-guest-john', N'john@staybnb.com', N'JOHN@STAYBNB.COM', N'john@staybnb.com', N'JOHN@STAYBNB.COM', 1, N'AQAAAAIAAYagAAAAELcsX80lPSAk1NYwXTsx2ZPexqE/OqaHBlyKkFa1ZBNYu/E4mYMzf1R9wOe/7DvdhA==',
     CONVERT(nvarchar(36), NEWID()), CONVERT(nvarchar(36), NEWID()), 0, 0, 1, 0, N'John', N'Marie', DATEADD(DAY, -40, GETDATE())),
    (N'demo-guest-courtney', N'courtney@staybnb.com', N'COURTNEY@STAYBNB.COM', N'courtney@staybnb.com', N'COURTNEY@STAYBNB.COM', 1, N'AQAAAAIAAYagAAAAEK0Vtrfe7P3UUzFJvH9pFs0H5+CSQ+tQ0S5RKdp+SApQ30gNl11o4qR5iq3IXhN2yw==',
     CONVERT(nvarchar(36), NEWID()), CONVERT(nvarchar(36), NEWID()), 0, 0, 1, 0, N'Courtney', N'Quibus', DATEADD(DAY, -4, GETDATE())),
    (N'demo-admin-royal', N'royal@staybnb.com', N'ROYAL@STAYBNB.COM', N'royal@staybnb.com', N'ROYAL@STAYBNB.COM', 1, N'AQAAAAIAAYagAAAAEEgL+1JJKhvo3hl8e+qrs+8vzO9Q61JcCdL+ostsIuWyqnUctyuBsQO/NGOsRu43Hg==',
     CONVERT(nvarchar(36), NEWID()), CONVERT(nvarchar(36), NEWID()), 0, 0, 1, 0, N'Royal', N'Admin', DATEADD(DAY, -70, GETDATE()));

-- Roles (looks up the role ids that the website created)
INSERT INTO dbo.AspNetUserRoles (UserId, RoleId) SELECT N'demo-host-gideon', Id FROM dbo.AspNetRoles WHERE Name = N'Host';
INSERT INTO dbo.AspNetUserRoles (UserId, RoleId) SELECT N'demo-host-marco', Id FROM dbo.AspNetRoles WHERE Name = N'Host';
INSERT INTO dbo.AspNetUserRoles (UserId, RoleId) SELECT N'demo-host-brandon', Id FROM dbo.AspNetRoles WHERE Name = N'Host';
INSERT INTO dbo.AspNetUserRoles (UserId, RoleId) SELECT N'demo-guest-reino', Id FROM dbo.AspNetRoles WHERE Name = N'Guest';
INSERT INTO dbo.AspNetUserRoles (UserId, RoleId) SELECT N'demo-guest-john', Id FROM dbo.AspNetRoles WHERE Name = N'Guest';
INSERT INTO dbo.AspNetUserRoles (UserId, RoleId) SELECT N'demo-guest-courtney', Id FROM dbo.AspNetRoles WHERE Name = N'Guest';
INSERT INTO dbo.AspNetUserRoles (UserId, RoleId) SELECT N'demo-admin-royal', Id FROM dbo.AspNetRoles WHERE Name = N'Admin';

-- =====================================================================================
-- 2. PROPERTIES (HostProperties). Ids 1001+ to prevent classh with self made data now.
--    Property types used by the website filter: Apartment, House, Cabin, Villa just for my test data
-- =====================================================================================
SET IDENTITY_INSERT dbo.HostProperties ON;
INSERT INTO dbo.HostProperties
    (Id, Title, Description, PricePerNight, CleaningFee, Address, City, PropertyType,
     MaxGuests, Bedrooms, Beds, Bathrooms, IsActive, CreatedAt, HostId)
VALUES
    (1001, N'Strand Beachfront Villa', N'Wake up to the sound of the waves. A spacious villa right on the Strand beachfront with a private pool, braai area and room for the whole family.', 2800.00, 500.00, N'12 Beach Road, Strand', N'Strand', N'Villa', 8, 4, 5, 3, 1, DATEADD(DAY, -55, GETDATE()), N'demo-host-gideon'),
    (1002, N'Strand Beach House', N'A relaxed beach house a short walk from the Strand promenade, with a sunny deck and a big open-plan kitchen.', 1900.00, 350.00, N'48 Beach Road, Strand', N'Strand', N'House', 6, 3, 4, 2, 1, DATEADD(DAY, -54, GETDATE()), N'demo-host-gideon'),
    (1003, N'Strand Seaview Apartment', N'Bright one-bedroom apartment with a balcony and a view of False Bay. Perfect for a weekend by the sea.', 1100.00, 250.00, N'5 Marine Drive, Strand', N'Strand', N'Apartment', 3, 1, 2, 1, 1, DATEADD(DAY, -50, GETDATE()), N'demo-host-marco'),
    (1004, N'Paarl Forest Cabin', N'A quiet cabin tucked between the trees outside Paarl. Fire pit, wooden deck and birdsong in the morning.', 1150.00, 200.00, N'Forest Lane, Paarl', N'Paarl', N'Cabin', 4, 2, 3, 1, 1, DATEADD(DAY, -48, GETDATE()), N'demo-host-marco'),
    (1005, N'Paarl Mountain Cabin', N'Cosy mountain cabin with a wood stove and sweeping views over the Paarl valley. Ideal for hiking weekends.', 1350.00, 250.00, N'Mountain View Road, Paarl', N'Paarl', N'Cabin', 4, 2, 2, 1, 1, DATEADD(DAY, -47, GETDATE()), N'demo-host-marco'),
    (1006, N'Somerset West Vineyard Villa', N'Elegant villa on a wine estate with mountain views, a heated pool and a large entertainment area.', 3200.00, 600.00, N'Vineyard Estate, Somerset West', N'Somerset West', N'Villa', 8, 4, 6, 3, 1, DATEADD(DAY, -45, GETDATE()), N'demo-host-brandon'),
    (1007, N'Somerset West Garden House', N'Peaceful family house set in a leafy garden, close to the Somerset West shops and restaurants.', 1450.00, 300.00, N'9 Protea Close, Somerset West', N'Somerset West', N'House', 5, 3, 3, 2, 0, DATEADD(DAY, -44, GETDATE()), N'demo-host-brandon'),
    (1008, N'Somerset West Modern Apartment', N'Modern apartment with secure parking, fast WiFi and everything you need for a work trip or a short break.', 980.00, 200.00, N'21 Main Road, Somerset West', N'Somerset West', N'Apartment', 3, 1, 2, 1, 1, DATEADD(DAY, -42, GETDATE()), N'demo-host-brandon'),
    (1009, N'Strand Surf Cottage', N'Small surf cottage two minutes from the beach, with an outdoor shower and a board storage room.', 1250.00, 250.00, N'7 Surf Street, Strand', N'Strand', N'House', 4, 2, 2, 1, 0, DATEADD(DAY, -1, GETDATE()), N'demo-guest-courtney');
SET IDENTITY_INSERT dbo.HostProperties OFF;

-- Photos (PropertyImages): files live in wwwroot\uploads  (i will upload these images so the detect if the script runs)
INSERT INTO dbo.PropertyImages (PropertyId, ImageUrl) VALUES
    (1001, N'/uploads/beach-villa-1.jpg'),
    (1001, N'/uploads/beach-villa-2.jpg'),
    (1002, N'/uploads/beach-house-1.jpg'),
    (1002, N'/uploads/beach-house-2.jpg'),
    (1003, N'/uploads/beach-apartment-1.jpg'),
    (1003, N'/uploads/beach-apartment-2.jpg'),
    (1004, N'/uploads/forest-cabin-1.jpg'),
    (1004, N'/uploads/forest-cabin-2.jpg'),
    (1005, N'/uploads/mountain-cabin-1.jpg'),
    (1005, N'/uploads/mountain-cabin-2.jpg'),
    (1006, N'/uploads/vineyard-villa-1.jpg'),
    (1006, N'/uploads/vineyard-villa-2.jpg'),
    (1007, N'/uploads/garden-house-1.jpg'),
    (1007, N'/uploads/garden-house-2.jpg'),
    (1008, N'/uploads/apartment-1.jpg'),
    (1008, N'/uploads/apartment-2.jpg'),
    (1009, N'/uploads/surf-cottage-1.jpg');

-- Amenities (many-to-many join table AmenityHostProperty; amenity ids are looked up by name)
INSERT INTO dbo.AmenityHostProperty (AmenitiesId, PropertiesId) SELECT Id, 1001 FROM dbo.Amenities WHERE Name IN (N'WiFi', N'Kitchen', N'Pool', N'Air Conditioning', N'Free Parking');
INSERT INTO dbo.AmenityHostProperty (AmenitiesId, PropertiesId) SELECT Id, 1002 FROM dbo.Amenities WHERE Name IN (N'WiFi', N'Kitchen', N'Washer', N'Free Parking');
INSERT INTO dbo.AmenityHostProperty (AmenitiesId, PropertiesId) SELECT Id, 1003 FROM dbo.Amenities WHERE Name IN (N'WiFi', N'Kitchen', N'Heating');
INSERT INTO dbo.AmenityHostProperty (AmenitiesId, PropertiesId) SELECT Id, 1004 FROM dbo.Amenities WHERE Name IN (N'Kitchen', N'Heating', N'Free Parking');
INSERT INTO dbo.AmenityHostProperty (AmenitiesId, PropertiesId) SELECT Id, 1005 FROM dbo.Amenities WHERE Name IN (N'WiFi', N'Kitchen', N'Heating', N'Free Parking');
INSERT INTO dbo.AmenityHostProperty (AmenitiesId, PropertiesId) SELECT Id, 1006 FROM dbo.Amenities WHERE Name IN (N'WiFi', N'Pool', N'Kitchen', N'Air Conditioning', N'Washer', N'Dryer', N'Free Parking');
INSERT INTO dbo.AmenityHostProperty (AmenitiesId, PropertiesId) SELECT Id, 1007 FROM dbo.Amenities WHERE Name IN (N'WiFi', N'Kitchen', N'Washer');
INSERT INTO dbo.AmenityHostProperty (AmenitiesId, PropertiesId) SELECT Id, 1008 FROM dbo.Amenities WHERE Name IN (N'WiFi', N'Air Conditioning', N'Kitchen');
INSERT INTO dbo.AmenityHostProperty (AmenitiesId, PropertiesId) SELECT Id, 1009 FROM dbo.Amenities WHERE Name IN (N'WiFi', N'Kitchen');

-- Host applications (Approved for every live property, plus one Pending for the Admin to review as an open tests still)
-- Status: 0 = Pending, 1 = Approved, 2 = Rejected
INSERT INTO dbo.HostApplications (ApplicationUserId, PropertyId, Status, AppliedAt) VALUES
    (N'demo-host-gideon', 1001, 1, DATEADD(DAY, -55, GETDATE())),
    (N'demo-host-gideon', 1002, 1, DATEADD(DAY, -54, GETDATE())),
    (N'demo-host-marco', 1003, 1, DATEADD(DAY, -50, GETDATE())),
    (N'demo-host-marco', 1004, 1, DATEADD(DAY, -48, GETDATE())),
    (N'demo-host-marco', 1005, 1, DATEADD(DAY, -47, GETDATE())),
    (N'demo-host-brandon', 1006, 1, DATEADD(DAY, -45, GETDATE())),
    (N'demo-host-brandon', 1007, 1, DATEADD(DAY, -44, GETDATE())),
    (N'demo-host-brandon', 1008, 1, DATEADD(DAY, -42, GETDATE())),
    (N'demo-guest-courtney', 1009, 0, DATEADD(DAY, -1, GETDATE()));

-- Check-in rules for each approved property (CheckInProcesses). Id = 4000 + last digits of property id (also copy pasted for the test data)
SET IDENTITY_INSERT dbo.CheckInProcesses ON;
INSERT INTO dbo.CheckInProcesses (Id, PropertyId, Title, StepsJson, CreatedAt) VALUES
    (4001, 1001, N'Standard check-in', N'["Upload a valid ID or Passport","Check-in is from 14:00","Please respect the house rules"]', DATEADD(DAY, -55, GETDATE())),
    (4002, 1002, N'Standard check-in', N'["Upload a valid ID or Passport","Check-in is from 14:00","Please respect the house rules"]', DATEADD(DAY, -54, GETDATE())),
    (4003, 1003, N'Standard check-in', N'["Upload a valid ID or Passport","Check-in is from 14:00","Please respect the house rules"]', DATEADD(DAY, -50, GETDATE())),
    (4004, 1004, N'Standard check-in', N'["Upload a valid ID or Passport","Check-in is from 14:00","Please respect the house rules"]', DATEADD(DAY, -48, GETDATE())),
    (4005, 1005, N'Standard check-in', N'["Upload a valid ID or Passport","Check-in is from 14:00","Please respect the house rules"]', DATEADD(DAY, -47, GETDATE())),
    (4006, 1006, N'Standard check-in', N'["Upload a valid ID or Passport","Check-in is from 14:00","Please respect the house rules"]', DATEADD(DAY, -45, GETDATE())),
    (4007, 1007, N'Standard check-in', N'["Upload a valid ID or Passport","Check-in is from 14:00","Please respect the house rules"]', DATEADD(DAY, -44, GETDATE())),
    (4008, 1008, N'Standard check-in', N'["Upload a valid ID or Passport","Check-in is from 14:00","Please respect the house rules"]', DATEADD(DAY, -42, GETDATE()));
SET IDENTITY_INSERT dbo.CheckInProcesses OFF;

-- =====================================================================================
-- 3. BOOKINGS in different statusses
--    Status: 0 = Pending, 1 = Approved, 2 = Rejected, 3 = CheckedIn
--    TotalPrice = worked out same as in backed just to get correct value here in tests data
-- =====================================================================================
SET IDENTITY_INSERT dbo.Bookings ON;
INSERT INTO dbo.Bookings (Id, PropertyId, GuestId, CheckInDate, CheckOutDate, NumberOfGuests, TotalPrice, Status, CreatedAt) VALUES
    (2001, 1001, N'demo-guest-reino', DATEADD(DAY, -25, CAST(GETDATE() AS date)), DATEADD(DAY, -22, CAST(GETDATE() AS date)), 4, 9908.00, 3, DATEADD(DAY, -28, GETDATE())),
    (2002, 1004, N'demo-guest-john', DATEADD(DAY, -15, CAST(GETDATE() AS date)), DATEADD(DAY, -13, CAST(GETDATE() AS date)), 2, 2776.00, 3, DATEADD(DAY, -18, GETDATE())),
    (2003, 1006, N'demo-guest-courtney', DATEADD(DAY, 12, CAST(GETDATE() AS date)), DATEADD(DAY, 15, CAST(GETDATE() AS date)), 4, 11352.00, 1, DATEADD(DAY, -3, GETDATE())),
    (2004, 1002, N'demo-guest-john', DATEADD(DAY, 20, CAST(GETDATE() AS date)), DATEADD(DAY, 22, CAST(GETDATE() AS date)), 5, 4606.00, 0, DATEADD(DAY, -3, GETDATE())),
    (2005, 1005, N'demo-guest-reino', DATEADD(DAY, 30, CAST(GETDATE() AS date)), DATEADD(DAY, 34, CAST(GETDATE() AS date)), 2, 6298.00, 0, DATEADD(DAY, -3, GETDATE())),
    (2006, 1003, N'demo-guest-courtney', DATEADD(DAY, 6, CAST(GETDATE() AS date)), DATEADD(DAY, 8, CAST(GETDATE() AS date)), 2, 2714.00, 2, DATEADD(DAY, -3, GETDATE()));
SET IDENTITY_INSERT dbo.Bookings OFF;

-- Payments (Status: 0 = Pending, 1 = Paid, 2 = Failed) also just random to show my diffrent statusses
INSERT INTO dbo.Payments (BookingId, Amount, Status, CreatedAt) VALUES
    (2001, 9908.00, 1, DATEADD(DAY, -28, GETDATE())),
    (2002, 2776.00, 1, DATEADD(DAY, -18, GETDATE())),
    (2003, 11352.00, 1, DATEADD(DAY, -3, GETDATE())),
    (2004, 4606.00, 0, DATEADD(DAY, -3, GETDATE())),
    (2005, 6298.00, 0, DATEADD(DAY, -3, GETDATE())),
    (2006, 2714.00, 2, DATEADD(DAY, -3, GETDATE()));

-- Guest check-ins for approved / checked-in bookings (Status: 0 = NotStarted, 1 = InProgress, 2 = Completed)
SET IDENTITY_INSERT dbo.GuestCheckIns ON;
INSERT INTO dbo.GuestCheckIns (Id, BookingId, CheckInProcessId, Status, CreatedAt) VALUES
    (3001, 2001, 4001, 2, DATEADD(DAY, -27, GETDATE())),
    (3002, 2002, 4004, 2, DATEADD(DAY, -17, GETDATE())),
    (3003, 2003, 4006, 2, DATEADD(DAY, -2, GETDATE()));
SET IDENTITY_INSERT dbo.GuestCheckIns OFF;

-- Guest documents (Status: 0 = Pending, 1 = Verified, 2 = Rejected)
-- i will place a file sample-id.png in the website folder  App_Data\documents for testing this
INSERT INTO dbo.GuestDocuments (GuestCheckInId, DocumentType, FileName, Status) VALUES
    (3001, N'ID', N'sample-id.png', 1),
    (3002, N'Passport', N'sample-id.png', 1),
    (3003, N'Passport', N'sample-id.png', 0);

-- =====================================================================================
-- 4. REVIEWS and WISHLIST (even though i did not implement this optional wishlist for my project)
-- =====================================================================================
INSERT INTO dbo.Reviews (PropertyId, ReviewerId, Rating, Comment, CreatedAt) VALUES
    (1001, N'demo-guest-reino', 5, N'Waking up to the sea was unreal. Spotless, and the host was super helpful.', DATEADD(DAY, -21, GETDATE()) ),
    (1004, N'demo-guest-john', 4, N'Quiet and peaceful, surrounded by trees. Cold nights, so bring warm clothes.', DATEADD(DAY, -12, GETDATE()) );

INSERT INTO dbo.WishlistItems (UserId, PropertyId, CreatedAt) VALUES
    (N'demo-guest-reino', 1006, DATEADD(DAY, -5, GETDATE())),
    (N'demo-guest-john', 1001, DATEADD(DAY, -4, GETDATE())),
    (N'demo-guest-courtney', 1005, DATEADD(DAY, -2, GETDATE()));

-- =====================================================================================
-- 5. MESSAGES (IsRead: 1 = read, 0 = unread)
-- =====================================================================================
INSERT INTO dbo.Messages (SenderId, ReceiverId, Content, IsRead, Timestamp) VALUES
    (N'demo-guest-reino',    N'demo-host-gideon',   N'Hi Gideon, is early check-in possible on the day we arrive?', 1, DATEADD(HOUR, -60, GETDATE())),
    (N'demo-host-gideon',    N'demo-guest-reino',   N'Hi Reino, yes, you can check in from 12:00. See you at the beach!', 0, DATEADD(HOUR, -59, GETDATE())),
    (N'demo-guest-john',     N'demo-host-marco',    N'Hello Marco, is the cabin in Paarl suitable for two adults and a dog?', 0, DATEADD(HOUR, -6, GETDATE())),
    (N'demo-guest-courtney', N'demo-host-brandon',  N'Hi Brandon, does the vineyard villa have a braai area?', 0, DATEADD(HOUR, -3, GETDATE())),
    (N'demo-host-brandon',   N'demo-guest-courtney',N'Hi Courtney, yes, there is a braai on the patio next to the pool.', 0, DATEADD(HOUR, -2, GETDATE()));

-- =====================================================================================
-- 6. NOTIFICATIONS (Type: 0 = Info, 1 = Success, 2 = Warning; IsRead: 1 = read, 0 = unread)
-- =====================================================================================
INSERT INTO dbo.Notifications (UserId, Title, Message, Type, Link, IsRead, CreatedAt) VALUES
    (N'demo-host-gideon', N'New booking request', N'A guest requested ''Strand Beach House''.', 2, N'/Host/BookingRequests', 0, DATEADD(HOUR, -5, GETDATE())),
    (N'demo-host-gideon', N'Host Application Approved', N'Congratulations! ''Strand Beachfront Villa'' is now live.', 1, N'/Host/Dashboard', 1, DATEADD(HOUR, -1300, GETDATE())),
    (N'demo-host-marco', N'New booking request', N'A guest requested ''Paarl Mountain Cabin''.', 2, N'/Host/BookingRequests', 0, DATEADD(HOUR, -4, GETDATE())),
    (N'demo-host-marco', N'New Message', N'New message from John Marie', 0, N'/Messages?with=demo-guest-john', 0, DATEADD(HOUR, -6, GETDATE())),
    (N'demo-host-brandon', N'Check-in ready to verify', N'A guest submitted documents for ''Somerset West Vineyard Villa''.', 2, N'/Host/CheckIns', 0, DATEADD(HOUR, -8, GETDATE())),
    (N'demo-host-brandon', N'New Message', N'New message from Courtney Quibus', 0, N'/Messages?with=demo-guest-courtney', 1, DATEADD(HOUR, -3, GETDATE())),
    (N'demo-guest-reino', N'New Message', N'New message from Gideon Pieterse', 0, N'/Messages?with=demo-host-gideon', 0, DATEADD(HOUR, -59, GETDATE())),
    (N'demo-guest-reino', N'Booking requested', N'Your request for ''Paarl Mountain Cabin'' is waiting for the host.', 0, N'/Booking/MyBookings', 1, DATEADD(HOUR, -70, GETDATE())),
    (N'demo-guest-john', N'Booking requested', N'Your request for ''Strand Beach House'' is waiting for the host.', 0, N'/Booking/MyBookings', 0, DATEADD(HOUR, -48, GETDATE())),
    (N'demo-guest-courtney', N'Booking Approved', N'Your booking for ''Somerset West Vineyard Villa'' was approved.', 1, N'/Booking/MyBookings', 0, DATEADD(HOUR, -30, GETDATE())),
    (N'demo-guest-courtney', N'Booking Rejected', N'Your booking for ''Strand Seaview Apartment'' was rejected.', 2, N'/Booking/MyBookings', 1, DATEADD(HOUR, -100, GETDATE())),
    (N'demo-guest-courtney', N'Application submitted', N'Your application for ''Strand Surf Cottage'' is under review.', 0, N'/HostApplication', 1, DATEADD(HOUR, -24, GETDATE())),
    (N'demo-admin-royal', N'New host application', N'''Strand Surf Cottage'' needs review.', 2, N'/Admin/Applications', 0, DATEADD(HOUR, -24, GETDATE()));

-- =====================================================================================
-- 7. ACTIVITY LOG (ActivityType: 0 Login, 1 Registration, 2 RoleChange, 3 Application, 4 Booking, 5 Property, 6 CheckIn)
-- all this will be random data and not my actual logs, they will show from usage when app is spun up
-- =====================================================================================
INSERT INTO dbo.ActivityLogs (UserId, Action, ActivityType, CreatedAt) VALUES
    (N'demo-admin-royal', N'Registered a new account', 1, DATEADD(DAY, -70, GETDATE())),
    (N'demo-host-gideon', N'Registered a new account', 1, DATEADD(DAY, -60, GETDATE())),
    (N'demo-host-marco', N'Registered a new account', 1, DATEADD(DAY, -58, GETDATE())),
    (N'demo-host-brandon', N'Registered a new account', 1, DATEADD(DAY, -55, GETDATE())),
    (N'demo-guest-reino', N'Registered a new account', 1, DATEADD(DAY, -45, GETDATE())),
    (N'demo-guest-john', N'Registered a new account', 1, DATEADD(DAY, -40, GETDATE())),
    (N'demo-guest-courtney', N'Registered a new account', 1, DATEADD(DAY, -4, GETDATE())),
    (N'demo-host-gideon', N'Role changed: Guest -> Host', 2, DATEADD(DAY, -55, GETDATE())),
    (N'demo-host-marco', N'Role changed: Guest -> Host', 2, DATEADD(DAY, -52, GETDATE())),
    (N'demo-host-brandon', N'Role changed: Guest -> Host', 2, DATEADD(DAY, -48, GETDATE())),
    (N'demo-admin-royal', N'Role changed: Guest -> Admin', 2, DATEADD(DAY, -65, GETDATE())),
    (N'demo-admin-royal', N'Approved application for Strand Beachfront Villa', 3, DATEADD(DAY, -55, GETDATE())),
    (N'demo-admin-royal', N'Approved application for Paarl Forest Cabin', 3, DATEADD(DAY, -48, GETDATE())),
    (N'demo-admin-royal', N'Approved application for Somerset West Vineyard Villa', 3, DATEADD(DAY, -45, GETDATE())),
    (N'demo-guest-reino', N'Logged in', 0, DATEADD(DAY, -3, GETDATE())),
    (N'demo-host-gideon', N'Logged in', 0, DATEADD(DAY, -2, GETDATE())),
    (N'demo-admin-royal', N'Logged in', 0, DATEADD(DAY, -1, GETDATE())),
    (N'demo-guest-reino', N'Requested booking #2001 for Strand Beachfront Villa', 4, DATEADD(DAY, -28, GETDATE())),
    (N'demo-host-gideon', N'Booking #2001 approved', 4, DATEADD(DAY, -27, GETDATE())),
    (N'demo-guest-reino', N'Submitted check-in for booking #2001', 6, DATEADD(DAY, -26, GETDATE())),
    (N'demo-host-gideon', N'Verified a document for booking #2001', 6, DATEADD(DAY, -26, GETDATE())),
    (N'demo-guest-john', N'Requested booking #2004 for Strand Beach House', 4, DATEADD(DAY, -2, GETDATE())),
    (N'demo-guest-courtney', N'Submitted check-in for booking #2003', 6, DATEADD(DAY, -1, GETDATE())),
    (N'demo-host-brandon', N'Deactivated Somerset West Garden House', 5, DATEADD(DAY, -10, GETDATE())),
    (N'demo-guest-courtney', N'Submitted host application: Strand Surf Cottage', 3, DATEADD(DAY, -1, GETDATE()));

COMMIT TRANSACTION;
PRINT N'Additional data loaded. Demo password for every demo user: Demo@123';