"""Registro de estudante (linha do dataset)."""

from dataclasses import dataclass


@dataclass
class Estudante:
    student_id: int
    age: int
    gender: str
    study_hours: float
    attendance: float
    sleep_hours: float
    previous_grade: float
    assignments_completed: float
    practice_tests_taken: float
    group_study_hours: float
    notes_quality_score: float
    time_management_score: float
    motivation_level: float
    mental_health_score: float
    screen_time: float
    social_media_hours: float
    family_income: str
    parent_education: str
    internet_access: str
    device_type: str
    school_type: str
    extracurriculars: str
    final_grade: float
    grade_category: str
    pass_fail: str

    def __str__(self):
        return (
            f"ID: {self.student_id} | Idade: {self.age} | Gênero: {self.gender} | "
            f"Nota Final: {self.final_grade:.2f} ({self.grade_category}) | "
            f"Resultado: {self.pass_fail} | Horas de Estudo: {self.study_hours:.2f}"
        )
