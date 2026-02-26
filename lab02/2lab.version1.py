import tkinter as tk
import customtkinter as ctk
from matplotlib.backends.backend_tkagg import FigureCanvasTkAgg
import matplotlib.pyplot as plt
import numpy as np
import time

# --- ЦВЕТОВАЯ ПАЛИТРА ---
BG_COLOR = "#121212"      # Глубокий черный для фона
CARD_COLOR = "#1E1E1E"    # Темно-серый для карточек
ACCENT_COLOR = "#3D5AFE"  # Яркий синий для кнопок
TEXT_COLOR = "#E0E0E0"    # Светло-серый для текста

ctk.set_appearance_mode("dark")

class HeatApp(ctk.CTk):
    def __init__(self):
        super().__init__()

        self.title("Thermal Analysis Pro — Steel")
        self.geometry("1280x850")
        self.configure(fg_color=BG_COLOR)
        self.protocol("WM_DELETE_WINDOW", self.on_closing)

        # Главная сетка
        self.grid_columnconfigure(1, weight=1)
        self.grid_rowconfigure(0, weight=1)

        # --- ЛЕВАЯ ПАНЕЛЬ (SIDEBAR) ---
        self.sidebar = ctk.CTkFrame(self, width=320, corner_radius=20, fg_color=CARD_COLOR, border_width=1, border_color="#333333")
        self.sidebar.grid(row=0, column=0, sticky="nsew", padx=20, pady=20)
        
        ctk.CTkLabel(self.sidebar, text="КОНФИГУРАЦИЯ", font=("Inter", 22, "bold"), text_color=ACCENT_COLOR).pack(pady=(30, 20))

        self.inputs = {}
        params = [
            ("Толщина L (м)", "0.1", "L"),
            ("T слева (°C)", "200", "Ta"),
            ("T справа (°C)", "20", "Tn"),
            ("T начальная (°C)", "20", "T0"),
            ("Шаг времени τ (с)", "0.01", "tau"),
            ("Шаг сетки h (м)", "0.01", "h")
        ]

        for label, default, var in params:
            f = ctk.CTkFrame(self.sidebar, fg_color="transparent")
            f.pack(fill="x", padx=25, pady=8)
            ctk.CTkLabel(f, text=label, font=("Inter", 12), text_color="#888888").pack(anchor="w")
            entry = ctk.CTkEntry(f, height=40, corner_radius=10, border_color="#444444", fg_color="#252525", font=("Consolas", 14))
            entry.insert(0, default)
            entry.pack(fill="x", pady=(5, 0))
            self.inputs[var] = entry

        self.btn_run = ctk.CTkButton(self.sidebar, text="ЗАПУСТИТЬ РАСЧЕТ", height=50, corner_radius=12, 
                                     fg_color=ACCENT_COLOR, hover_color="#2A3EB1", font=("Inter", 15, "bold"),
                                     command=self.run_simulation)
        self.btn_run.pack(pady=(40, 10), padx=25, fill="x")

        self.btn_clear = ctk.CTkButton(self.sidebar, text="ОЧИСТИТЬ ТАБЛИЦУ", height=40, corner_radius=10, 
                                       fg_color="transparent", border_width=1, border_color="#555555",
                                       text_color="#AAAAAA", hover_color="#333333", command=self.clear_table)
        self.btn_clear.pack(pady=10, padx=25, fill="x")

        # --- ПРАВАЯ ЧАСТЬ ---
        self.right_frame = ctk.CTkFrame(self, fg_color="transparent")
        self.right_frame.grid(row=0, column=1, sticky="nsew", padx=(0, 20), pady=20)
        self.right_frame.grid_rowconfigure(0, weight=6)
        self.right_frame.grid_rowconfigure(1, weight=4)

        # 1. График (В белой карточке для контраста)
        self.plot_card = ctk.CTkFrame(self.right_frame, corner_radius=20, fg_color="white")
        self.plot_card.grid(row=0, column=0, sticky="nsew", pady=(0, 20))
        
        # Стилизация графика Matplotlib под минимализм
        self.fig, self.ax = plt.subplots(figsize=(5, 3), dpi=100)
        self.fig.patch.set_facecolor('white')
        self.ax.spines['top'].set_visible(False)
        self.ax.spines['right'].set_visible(False)
        
        self.canvas = FigureCanvasTkAgg(self.fig, master=self.plot_card)
        self.canvas.get_tk_widget().pack(fill="both", expand=True, padx=20, pady=20)

        # 2. Таблица с результатами
        self.table_card = ctk.CTkFrame(self.right_frame, corner_radius=20, fg_color=CARD_COLOR, border_width=1, border_color="#333333")
        self.table_card.grid(row=1, column=0, sticky="nsew")
        
        # Заголовок таблицы
        header_f = ctk.CTkFrame(self.table_card, fg_color="#252525", height=50, corner_radius=0)
        header_f.pack(fill="x", padx=2, pady=2)
        cols = ["Шаг τ (с)", "Шаг h (м)", "T центр (°C)", "Время (с)"]
        for i, text in enumerate(cols):
            ctk.CTkLabel(header_f, text=text, font=("Inter", 12, "bold"), text_color="#888888", width=200).grid(row=0, column=i, sticky="we")

        self.scroll_rows = ctk.CTkScrollableFrame(self.table_card, fg_color="transparent", corner_radius=0)
        self.scroll_rows.pack(fill="both", expand=True, padx=5, pady=5)
        self.row_count = 0

    def run_simulation(self):
        try:
            L, Ta, Tn, T0 = float(self.inputs['L'].get()), float(self.inputs['Ta'].get()), float(self.inputs['Tn'].get()), float(self.inputs['T0'].get())
            h, tau = float(self.inputs['h'].get()), float(self.inputs['tau'].get())
            
            rho, c, lam, total_time = 7800.0, 460.0, 46.0, 2.0
            x = np.arange(0, L + h/2, h)
            N = len(x)
            T = np.full(N, T0)
            T[0], T[-1] = Ta, Tn

            Ai = lam / (h**2)
            Ci = lam / (h**2)
            Bi = (2 * lam / (h**2)) + (rho * c / tau)
            
            start_t = time.time()
            for _ in range(int(total_time / tau)):
                alpha, beta = np.zeros(N), np.zeros(N)
                alpha[0], beta[0] = 0, Ta
                for i in range(1, N - 1):
                    Fi = -(rho * c / tau) * T[i]
                    denom = Bi - Ci * alpha[i-1]
                    alpha[i] = Ai / denom
                    beta[i] = (Ci * beta[i-1] - Fi) / denom
                T_next = np.zeros(N)
                T_next[-1] = Tn
                for i in range(N - 2, 0, -1):
                    T_next[i] = alpha[i] * T_next[i+1] + beta[i]
                T_next[0], T = Ta, T_next
            
            exec_t = time.time() - start_t
            center_t = T[N//2]

            # График
            self.ax.clear()
            self.ax.plot(x, T, color=ACCENT_COLOR, linewidth=3, label="Final State")
            self.ax.fill_between(x, T, 20, color=ACCENT_COLOR, alpha=0.1) # Легкая заливка под графиком
            self.ax.set_title("Distribution Profile", fontdict={'fontsize': 16, 'fontweight': 'bold'})
            self.ax.grid(True, linestyle=':', alpha=0.4)
            self.canvas.draw()

            # Добавление в таблицу с чередованием цвета
            bg = "#2A2A2A" if self.row_count % 2 == 0 else "transparent"
            r = ctk.CTkFrame(self.scroll_rows, fg_color=bg, height=40, corner_radius=8)
            r.pack(fill="x", pady=2, padx=5)
            
            vals = [f"{tau:.4f}", f"{h:.4f}", f"{center_t:.4f}", f"{exec_t:.4f}"]
            for i, v in enumerate(vals):
                ctk.CTkLabel(r, text=v, width=200, font=("Consolas", 13)).grid(row=0, column=i)
            self.row_count += 1

        except Exception as e:
            tk.messagebox.showerror("Error", f"Invalid input: {e}")

    def clear_table(self):
        for child in self.scroll_rows.winfo_children():
            child.destroy()
        self.row_count = 0

    def on_closing(self):
        self.quit()
        self.destroy()

if __name__ == "__main__":
    app = HeatApp()
    app.mainloop()