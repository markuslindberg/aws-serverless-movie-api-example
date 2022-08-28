namespace MovieApi.Domain;

public record Movie(string MovieId, string Title, int Year, string Category, string Budget, string BoxOffice);

public record Character(string CharacterId, string Name, string PlayedBy, string Role, string Nationality);

public record Director(string DirectorId, string Name);