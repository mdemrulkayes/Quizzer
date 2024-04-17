﻿using Modules.Question.Application.Question.QuestionSet.Dtos;
using SharedKernel.Core;

namespace Modules.Question.Application.Question.QuestionSet.Query;
public sealed record GetQuestionSetByIdQuery(long QuestionSetId) : IQuery<Result<QuestionSetResponse>>;