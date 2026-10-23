namespace _402410745_Staybnb_assignment.Models;

public enum ApplicationStatus { Pending, Approved, Rejected }
public enum BookingStatus { Pending, Approved, Rejected, CheckedIn }
public enum PaymentStatus { Pending, Paid, Failed }
public enum NotificationType { Info, Success, Warning }
public enum ActivityType { Login, Registration, RoleChange, Application, Booking, Property, CheckIn }
public enum CheckInStatus { NotStarted, InProgress, Completed }
public enum DocumentStatus { Pending, Verified, Rejected }
