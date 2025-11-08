using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.Quiz.Application.Question.Question.Dtos;

public sealed record QuestionOptionResponse(long QuestionOptionId, string OptionText, bool IsCorrect);
