Architecture diagram with the `.aspx.cs` (Code-Behind) files explicitly included. In ASP.NET Web Forms, every `.aspx`, `.ascx`, and `.Master` file has a paired `.cs` file right underneath it that handles its specific events.

```text
WebLearningSystem/
│
├── App_Code/                
│   ├── BLL/                 
│   │   ├── CourseManager.cs
│   │   └── QuizEvaluator.cs
│   ├── DAL/                 
│   │   ├── DatabaseHelper.cs
│   │   └── QuizRepository.cs
│   └── Models/              
│       ├── User.cs
│       └── Quiz.cs
│
├── Assets/                  
│   ├── CSS/                 
│   ├── JS/                  
│   └── Images/              
│
├── Modules/                 
│   ├── Admin/               
│   │   ├── Dashboard.aspx
│   │   ├── Dashboard.aspx.cs
│   │   ├── AuditLogs.aspx
│   │   └── AuditLogs.aspx.cs
│   ├── Learner/             
│   │   ├── CodeLab.aspx
│   │   └── CodeLab.aspx.cs
│   ├── Tutor/               
│   │   ├── ConfigureQuiz.aspx
│   │   └── ConfigureQuiz.aspx.cs
│   └── Public/              
│       ├── Login.aspx
│       └── Login.aspx.cs
│
├── UserControls/            
│   ├── LeaderboardWidget.ascx
│   └── LeaderboardWidget.ascx.cs
│
├── Site.Master              
├── Site.Master.cs           
└── Web.config               

```

---

## Folder Responsibilities

**App_Code/Models/**
This folder holds plain C# classes that represent your data. A `Quiz.cs` file here would just contain properties like `QuizID`, `Title`, and `PassMarkPercent`. It carries data between your layers.

**App_Code/DAL/ (Data Access Layer)**
This folder is strictly for database connectivity to your local database. Files here (like `QuizRepository.cs`) contain the actual SQL queries to insert, display, update, and delete records. They use `SqlConnection` and `SqlCommand`. **No UI code goes here.**

**App_Code/BLL/ (Business Logic Layer)**
This folder contains the "brain" of your application. Files here (like `QuizEvaluator.cs`) enforce your system's rules. If a Tutor tries to set a passing score to 150%, the BLL catches that error before it reaches the database. **No SQL queries and no UI code go here.**

**Assets/**
This holds your external CSS stylesheets, JavaScript files, and images.

**Modules/**
This is your Presentation Layer, organized by user roles to support your team's workflow.

* **The `.aspx` files:** Contain the HTML5 elements, ASP.NET server controls, and inline/internal CSS.


* **The `.aspx.cs` files (Code-Behind):** Handle page loads and button clicks. They collect data from the screen and pass it to the BLL. **No raw SQL queries go here.**

**UserControls/**
This folder holds reusable UI snippets. If you want the Gamification Leaderboard to appear on multiple pages, you build it once here as an `.ascx` file and drop it into whichever `.aspx` pages need it.

---

## Example Flow: A Tutor Creates a New Quiz

To help your team understand how these folders interact without turning into spaghetti code, share this step-by-step flow of a Tutor creating a quiz.

1. **The User Interface (`Modules/Tutor/ConfigureQuiz.aspx`)**
The Tutor fills out a form with the quiz title and passing score, then clicks the "Save Quiz" button.
2. **The Code-Behind (`Modules/Tutor/ConfigureQuiz.aspx.cs`)**
This file catches the button click event. It pulls the text from the form fields, packs it into a `Quiz` model object, and sends it to the Business Logic Layer.

```csharp
protected void btnSaveQuiz_Click(object sender, EventArgs e)
{
    Quiz newQuiz = new Quiz();
    newQuiz.Title = txtQuizTitle.Text;
    newQuiz.PassMark = Convert.ToInt32(txtPassMark.Text);

    QuizEvaluator evaluator = new QuizEvaluator();
    bool isSuccess = evaluator.ValidateAndSaveQuiz(newQuiz);
    
    // Update the UI with a success or error message
}

```

3. **The Business Logic (`App_Code/BLL/QuizEvaluator.cs`)**
This file receives the `Quiz` object. It checks if the title is blank or if the passing score is invalid. If everything is correct, it passes the object down to the Data Access Layer.

```csharp
public bool ValidateAndSaveQuiz(Quiz quizData)
{
    if (quizData.PassMark < 0 || quizData.PassMark > 100) return false;
    
    QuizRepository repo = new QuizRepository();
    return repo.InsertQuiz(quizData);
}

```

4. **The Data Access (`App_Code/DAL/QuizRepository.cs`)**
This file receives the validated data and executes the SQL `INSERT` command. It connects to the database, runs the query, and returns a `true` or `false` back up the chain.



```csharp
public bool InsertQuiz(Quiz quizData)
{
    // Write the raw SQL INSERT statement here using parameterized queries
    // Execute the command and return true if rows were affected
}

```

By keeping these steps strictly separated, your teammate working on the Tutor frontend never has to worry about breaking the database connection code!