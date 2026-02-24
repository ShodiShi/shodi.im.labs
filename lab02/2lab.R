# Установи библиотеки, если их нет:
# install.packages("shiny")
# install.packages("shinythemes")

library(shiny)
library(shinythemes)

# --- 1. ФИЗИЧЕСКАЯ ЛОГИКА (БЕЗ ИЗМЕНЕНИЙ) ---
simulate_heat <- function(tau, h, total_time = 2) {
  rho <- 7800; c_val <- 460; lambda <- 46; L <- 0.1
  Ta <- 100; Tn <- 20; T0 <- 20
  x_points <- seq(0, L, by = h)
  N <- length(x_points)
  T_curr <- rep(T0, N)
  T_curr[1] <- Ta; T_curr[N] <- Tn
  Ai <- lambda / (h^2); Ci <- lambda / (h^2)
  Bi <- (2 * lambda / (h^2)) + (rho * c_val / tau)
  steps <- floor(total_time / tau)
  
  for (n in 1:steps) {
    alpha <- numeric(N); beta <- numeric(N)
    alpha[1] <- 0; beta[1] <- Ta
    for (i in 2:(N-1)) {
      Fi <- -(rho * c_val / tau) * T_curr[i]
      denom <- Bi - Ci * alpha[i-1]
      if (is.na(denom) || abs(denom) < 1e-20) denom <- 1e-20
      alpha[i] <- Ai / denom
      beta[i] <- (Ci * beta[i-1] - Fi) / denom
    }
    T_next <- numeric(N); T_next[N] <- Tn
    for (i in (N-1):2) { T_next[i] <- alpha[i] * T_next[i+1] + beta[i] }
    T_next[1] <- Ta; T_curr <- T_next
  }
  return(list(x = x_points, temp = T_curr))
}

# --- 2. СТИЛЬНЫЙ ИНТЕРФЕЙС (UI) ---
ui <- fluidPage(
  theme = shinytheme("flatly"), # Используем стильную тему Flatly
  
  # Кастомный CSS для красоты
  tags$head(
    tags$style(HTML("
      body { background-color: #f4f7f6; }
      .well { background-color: #ffffff; border-radius: 15px; box-shadow: 0 4px 12px rgba(0,0,0,0.1); }
      .btn-primary { background-color: #2c3e50; border: none; border-radius: 8px; transition: 0.3s; }
      .btn-primary:hover { background-color: #1a252f; transform: translateY(-2px); }
      .shiny-plot-output { border-radius: 15px; overflow: hidden; }
      h2 { color: #2c3e50; font-weight: bold; margin-bottom: 25px; }
      .table { background: white; border-radius: 10px; overflow: hidden; }
    "))
  ),
  
  titlePanel("🔥 Термодинамическое моделирование"),
  
  sidebarLayout(
    sidebarPanel(
      h4("⚙️ Параметры сетки", style="color: #2c3e50;"),
      numericInput("h_val", "Шаг по пространству (h), м:", value = 0.01, min = 0.0001, step = 0.001),
      hr(),
      h4("⏳ Сравнение шагов времени", style="color: #2c3e50;"),
      numericInput("tau1", "Шаг 1 (базовый):", value = 0.1),
      numericInput("tau2", "Шаг 2 (точнее):", value = 0.01),
      numericInput("tau3", "Шаг 3 (максимум):", value = 0.001),
      br(),
      actionButton("run", "ЗАПУСТИТЬ АНАЛИЗ", class = "btn-primary btn-lg", style="width: 100%; color: white;")
    ),
    
    mainPanel(
      tabsetPanel(
        tabPanel("📈 График сравнения", 
                 br(),
                 plotOutput("comparePlot", height = "450px"),
                 br(),
                 h4("📊 Итоговые данные (центр пластины):"),
                 tableOutput("resTable")
        ),
        tabPanel("ℹ️ О методе", 
                 br(),
                 wellPanel(
                   p("Используется ", strong("неявная конечно-разностная схема"), " и ", strong("метод прогонки"), "."),
                   p("Этот метод абсолютно устойчив, что позволяет сравнивать точность при любых шагах времени без риска 'взрыва' решения.")
                 )
        )
      )
    )
  )
)

# --- 3. СЕРВЕРНАЯ ЛОГИКА ---
server <- function(input, output) {
  observeEvent(input$run, {
    # Расчеты
    res1 <- simulate_heat(input$tau1, input$h_val)
    res2 <- simulate_heat(input$tau2, input$h_val)
    res3 <- simulate_heat(input$tau3, input$h_val)
    
    # Красивый график
    output$comparePlot <- renderPlot({
      par(mar = c(5, 5, 4, 2) + 0.1)
      plot(res1$x, res1$temp, type="l", col="#e74c3c", lwd=3, 
           xlab="Толщина пластины (м)", ylab="Температура (°C)", 
           main="Распределение температуры через t = 2 сек",
           cex.main=1.5, col.main="#2c3e50", axes=FALSE)
      
      axis(1, col="#2c3e50"); axis(2, col="#2c3e50")
      lines(res2$x, res2$temp, col="#3498db", lwd=3, lty=2)
      lines(res3$x, res3$temp, col="#2ecc71", lwd=3, lty=3)
      
      legend("topright", 
             legend=c(paste("τ =", input$tau1), 
                      paste("τ =", input$tau2), 
                      paste("τ =", input$tau3)),
             col=c("#e74c3c", "#3498db", "#2ecc71"), 
             lty=1:3, lwd=3, bty="n", cex=1.2)
      grid(col = "gray90")
    })
    
    # Таблица результатов
    output$resTable <- renderTable({
      get_mid_val <- function(res) {
        mid_idx <- floor(length(res$temp) / 2) + 1
        return(round(res$temp[mid_idx], 4))
      }
      
      data.frame(
        "Вариант расчета" = c("Пример 1 (грубый)", "Пример 2 (средний)", "Пример 3 (точный)"),
        "Шаг_времени_tau" = c(input$tau1, input$tau2, input$tau3),
        "Температура_в_центре_C" = c(get_mid_val(res1), get_mid_val(res2), get_mid_val(res3))
      )
    }, striped = TRUE, hover = TRUE, bordered = TRUE)
  })
}

# Запуск
shinyApp(ui = ui, server = server)