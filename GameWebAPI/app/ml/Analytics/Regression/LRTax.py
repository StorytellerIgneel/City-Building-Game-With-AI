import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns
import time

from sklearn.model_selection import cross_val_score, train_test_split
from sklearn.linear_model import LinearRegression
from sklearn.metrics import mean_squared_error, mean_absolute_error, r2_score

# =========================================================
# 1. READ CSV
# =========================================================

csv_path = r"C:\UnityProjects\FYP\GameWebAPI\app\ml\Analytics\Preprocessed\session_features.csv"

pd.set_option("display.max_columns", None)
pd.set_option("display.width", None)
pd.set_option("display.max_rows", None)
pd.set_option("display.max_colwidth", None)

df = pd.read_csv(csv_path)

print("-" * 60)
print("Original dataset shape:", df.shape)
print(df.head())

# =========================================================
# 2. CLEANING
# =========================================================

df = df.drop_duplicates().copy()
df = df.replace([np.inf, -np.inf], np.nan)

print("-" * 60)
print("Shape after basic cleaning:", df.shape)

# =========================================================
# 3. FEATURE / TARGET SELECTION
#    IMPORTANT: use columns that actually exist in session_features.csv
# =========================================================

target_col = "final_tax_income"

feature_cols = [
    "avg_action_intensity",
    #"std_action_intensity",
    # "avg_house_per_turn",
    # "avg_factory_per_turn",
    # "avg_service_per_turn",
    # "avg_supply_per_turn",
    # "avg_road_per_turn",
    "avg_population_growth",
    "avg_gold_growth",
    "avg_supply_growth",
    "avg_ap_growth",
    "avg_satisfaction_growth",
    "avg_service_growth",
    "avg_pollution_growth",
    # "avg_tax_income_growth",
    # "avg_satisfaction_index",
    # "avg_service_index",
    # "avg_pollution_index",
    #"total_population_growth",
    #"total_gold_growth",
    #"total_supply_growth",
    #"total_ap_growth",
    "supply_efficiency",
    # "avg_supply_ratio",
    #"min_supply_ratio",
    # "service_to_house",
    # "factory_to_house",
    # "used_service_early",
    # "used_factory_early",
    # "early_service_ratio",
    # "early_factory_ratio"
]

corr_matrix = df[feature_cols].corr(numeric_only=True)

import seaborn as sns
import matplotlib.pyplot as plt

plt.figure(figsize=(10, 8))
sns.heatmap(
    corr_matrix,
    annot=True,
    fmt=".2f",
    cmap="coolwarm",
    vmin=-1,
    vmax=1,
    linewidths=0.5
)
plt.title("Feature-to-Feature Correlation Matrix")
plt.tight_layout()
plt.show()

missing_cols = [col for col in feature_cols + [target_col] if col not in df.columns]
if missing_cols:
    raise ValueError(f"These columns are missing from the dataframe: {missing_cols}")

df_model = df.dropna(subset=feature_cols + [target_col]).copy()

print("-" * 60)
print("Shape after dropping NA for selected columns:", df_model.shape)

X = df_model[feature_cols]
y = df_model[target_col]


# =========================================================
# 4. SPLIT INTO TRAIN / VALIDATION / TEST
# =========================================================

# First split: train+temp and test
X_train, X_temp, y_train, y_temp = train_test_split(
    X, y, test_size=0.30, random_state=42
)

# Second split: validation and test
X_validation, X_test, y_validation, y_test = train_test_split(
    X_temp, y_temp, test_size=0.50, random_state=42
)

print("-" * 60)
print(f"Train shape: {X_train.shape}")
print(f"Validation shape: {X_validation.shape}")
print(f"Test shape: {X_test.shape}")

# =========================================================
# 5. CORRELATION CHECK
# =========================================================

corr_with_target = df_model[feature_cols + [target_col]].corr(numeric_only=True)[target_col].drop(target_col)
corr_sorted = corr_with_target.reindex(corr_with_target.abs().sort_values(ascending=False).index)

print("-" * 60)
print(f"Correlation with {target_col}:")
print(corr_sorted)

plt.figure(figsize=(12, 6))
corr_sorted.plot(kind="bar")
plt.title(f"Feature Correlation with {target_col}")
plt.ylabel("Correlation")
plt.xticks(rotation=45, ha="right")
plt.tight_layout()
plt.show()

# =========================================================
# 6. REGRESSION MODEL LOOP
#    Similar style to your classifier evaluation loop
# =========================================================

regressors = {
    "Linear Regression": LinearRegression()
}

scores = cross_val_score(regressors["Linear Regression"], X, y, cv=5, scoring="r2")

print("Cross-validation R² scores:", scores)
print("Mean R²:", scores.mean())

for regressor_name, regressor in regressors.items():
    start_time = time.time()

    # Train
    regressor.fit(X_train, y_train)

    # -------------------------
    # Validation evaluation
    # -------------------------
    y_pred_validation = regressor.predict(X_validation)

    mse_validation = mean_squared_error(y_validation, y_pred_validation)
    rmse_validation = np.sqrt(mse_validation)
    mae_validation = mean_absolute_error(y_validation, y_pred_validation)
    r2_validation = r2_score(y_validation, y_pred_validation)

    print("-" * 60)
    print(f"{regressor_name} - Validation Results")
    print(f"MSE  : {mse_validation:.4f}")
    print(f"RMSE : {rmse_validation:.4f}")
    print(f"MAE  : {mae_validation:.4f}")
    print(f"R²   : {r2_validation:.4f}")

    validation_results = pd.DataFrame({
        "Actual": y_validation.values,
        "Predicted": y_pred_validation
    })
    print("\nValidation Predictions:")
    print(validation_results)

    # -------------------------
    # Test evaluation
    # -------------------------
    y_pred_test = regressor.predict(X_test)

    mse_test = mean_squared_error(y_test, y_pred_test)
    rmse_test = np.sqrt(mse_test)
    mae_test = mean_absolute_error(y_test, y_pred_test)
    r2_test = r2_score(y_test, y_pred_test)

    print("-" * 60)
    print(f"{regressor_name} - Test Results")
    print(f"MSE  : {mse_test:.4f}")
    print(f"RMSE : {rmse_test:.4f}")
    print(f"MAE  : {mae_test:.4f}")
    print(f"R²   : {r2_test:.4f}")

    test_results = pd.DataFrame({
        "Actual": y_test.values,
        "Predicted": y_pred_test
    })
    print("\nTest Predictions:")
    print(test_results)

    # -------------------------
    # Coefficient analysis
    # -------------------------
    coeff_df = pd.DataFrame({
        "Feature": feature_cols,
        "Coefficient": regressor.coef_
    }).sort_values(by="Coefficient", key=np.abs, ascending=False)

    print("-" * 60)
    print(f"{regressor_name} - Top Influential Features")
    print(coeff_df.head(15))

    end_time = time.time()
    execution_time = end_time - start_time

    print("-" * 60)
    print(f"Execution time for {regressor_name}: {execution_time:.2f} seconds")

    # -------------------------
    # Validation plot
    # -------------------------
    plt.figure(figsize=(6, 6))
    plt.scatter(y_validation, y_pred_validation)
    plt.xlabel("Actual Population (Validation)")
    plt.ylabel("Predicted Population")
    plt.title(f"{regressor_name}: Validation Actual vs Predicted")
    plt.plot(
        [y_validation.min(), y_validation.max()],
        [y_validation.min(), y_validation.max()],
        'r--'
    )
    plt.tight_layout()
    plt.show()

    # -------------------------
    # Test plot
    # -------------------------
    plt.figure(figsize=(6, 6))
    plt.scatter(y_test, y_pred_test)
    plt.xlabel("Actual Population (Test)")
    plt.ylabel("Predicted Population")
    plt.title(f"{regressor_name}: Test Actual vs Predicted")
    plt.plot(
        [y_test.min(), y_test.max()],
        [y_test.min(), y_test.max()],
        'r--'
    )
    plt.tight_layout()
    plt.show()