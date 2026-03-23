using CsvHelper.Configuration.Attributes;

namespace ProjetoArvoreBinaria.Models;

/// <summary>Linha do dataset (chave de ordenação nas árvores: StudentId).</summary>
public sealed class Estudante
{
    [Name("student_id")]
    public int StudentId { get; set; }

    [Name("age")]
    public int Age { get; set; }

    [Name("gender")]
    public string Gender { get; set; } = "";

    [Name("study_hours")]
    public double StudyHours { get; set; }

    [Name("attendance")]
    public double Attendance { get; set; }

    [Name("sleep_hours")]
    public double SleepHours { get; set; }

    [Name("previous_grade")]
    public double PreviousGrade { get; set; }

    [Name("assignments_completed")]
    public double AssignmentsCompleted { get; set; }

    [Name("practice_tests_taken")]
    public double PracticeTestsTaken { get; set; }

    [Name("group_study_hours")]
    public double GroupStudyHours { get; set; }

    [Name("notes_quality_score")]
    public double NotesQualityScore { get; set; }

    [Name("time_management_score")]
    public double TimeManagementScore { get; set; }

    [Name("motivation_level")]
    public double MotivationLevel { get; set; }

    [Name("mental_health_score")]
    public double MentalHealthScore { get; set; }

    [Name("screen_time")]
    public double ScreenTime { get; set; }

    [Name("social_media_hours")]
    public double SocialMediaHours { get; set; }

    [Name("family_income")]
    public string FamilyIncome { get; set; } = "";

    [Name("parent_education")]
    public string ParentEducation { get; set; } = "";

    [Name("internet_access")]
    public string InternetAccess { get; set; } = "";

    [Name("device_type")]
    public string DeviceType { get; set; } = "";

    [Name("school_type")]
    public string SchoolType { get; set; } = "";

    [Name("extracurriculars")]
    public string Extracurriculars { get; set; } = "";

    [Name("final_grade")]
    public double FinalGrade { get; set; }

    [Name("grade_category")]
    public string GradeCategory { get; set; } = "";

    [Name("pass_fail")]
    public string PassFail { get; set; } = "";

    public override string ToString() =>
        $"ID: {StudentId} | Idade: {Age} | Gênero: {Gender} | " +
        $"Nota Final: {FinalGrade:F2} ({GradeCategory}) | " +
        $"Resultado: {PassFail} | Horas de Estudo: {StudyHours:F2}";
}
