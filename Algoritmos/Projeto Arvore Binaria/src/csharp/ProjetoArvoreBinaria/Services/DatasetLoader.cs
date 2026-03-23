using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using ProjetoArvoreBinaria.Models;
using ProjetoArvoreBinaria.Trees;

namespace ProjetoArvoreBinaria.Services;

public static class DatasetLoader
{
    public static (ArvoreBinariaBusca Abb, ArvoreAvl Avl, double TAbbSeg, double TAvlSeg, int NLinhas) Carregar(string caminho)
    {
        var ext = Path.GetExtension(caminho).ToLowerInvariant();
        var estudantes = ext == ".csv" ? CarregarCsv(caminho) : CarregarExcel(caminho);
        var n = estudantes.Count;

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var abb = new ArvoreBinariaBusca();
        foreach (var e in estudantes) abb.Inserir(e);
        sw.Stop();
        var tAbb = sw.Elapsed.TotalSeconds;

        sw.Restart();
        var avl = new ArvoreAvl();
        foreach (var e in estudantes) avl.Inserir(e);
        sw.Stop();
        var tAvl = sw.Elapsed.TotalSeconds;

        return (abb, avl, tAbb, tAvl, n);
    }

    private static List<Estudante> CarregarCsv(string caminho)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            BadDataFound = null
        };
        using var reader = new StreamReader(caminho, Encoding.UTF8);
        using var csv = new CsvReader(reader, config);
        return csv.GetRecords<Estudante>().ToList();
    }

    private static List<Estudante> CarregarExcel(string caminho)
    {
        using var wb = new XLWorkbook(caminho);
        var ws = wb.Worksheet(1);
        var primeira = ws.FirstRowUsed() ?? throw new InvalidOperationException("Planilha vazia.");
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var cell in primeira.CellsUsed())
            map[cell.GetString().Trim()] = cell.Address.ColumnNumber;

        int Ix(string nome) => map.TryGetValue(nome, out var i) ? i : throw new InvalidOperationException($"Coluna ausente: {nome}");

        var lista = new List<Estudante>();
        foreach (var row in ws.RowsUsed())
        {
            if (row.RowNumber() == 1) continue;
            if (row.IsEmpty()) continue;
            double D(string n) => double.TryParse(row.Cell(Ix(n)).GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : 0;
            int Int(string n) => int.TryParse(row.Cell(Ix(n)).GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : 0;
            lista.Add(new Estudante
            {
                StudentId = Int("student_id"),
                Age = Int("age"),
                Gender = row.Cell(Ix("gender")).GetString(),
                StudyHours = D("study_hours"),
                Attendance = D("attendance"),
                SleepHours = D("sleep_hours"),
                PreviousGrade = D("previous_grade"),
                AssignmentsCompleted = D("assignments_completed"),
                PracticeTestsTaken = D("practice_tests_taken"),
                GroupStudyHours = D("group_study_hours"),
                NotesQualityScore = D("notes_quality_score"),
                TimeManagementScore = D("time_management_score"),
                MotivationLevel = D("motivation_level"),
                MentalHealthScore = D("mental_health_score"),
                ScreenTime = D("screen_time"),
                SocialMediaHours = D("social_media_hours"),
                FamilyIncome = row.Cell(Ix("family_income")).GetString(),
                ParentEducation = row.Cell(Ix("parent_education")).GetString(),
                InternetAccess = row.Cell(Ix("internet_access")).GetString(),
                DeviceType = row.Cell(Ix("device_type")).GetString(),
                SchoolType = row.Cell(Ix("school_type")).GetString(),
                Extracurriculars = row.Cell(Ix("extracurriculars")).GetString(),
                FinalGrade = D("final_grade"),
                GradeCategory = row.Cell(Ix("grade_category")).GetString(),
                PassFail = row.Cell(Ix("pass_fail")).GetString()
            });
        }
        return lista;
    }
}
