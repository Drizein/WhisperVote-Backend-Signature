namespace Domain.Entities;

public class UserVote : _BaseEntity
{
    public string UserId { get; set; }

    public string SurveyId { get; set; }

    public Guid Guid => _guid;

    public UserVote(string userId, string surveyId)
    {
        UserId = userId;
        SurveyId = surveyId;
    }
}