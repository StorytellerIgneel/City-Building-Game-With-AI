import re
from pathlib import Path

import numpy as np
import pandas as pd

# ========= CONFIG =========
INPUT_FOLDER = Path(r"C:\UnityProjects\FYP\Analytics\Logs\TurnSnapshots")
OUTPUT_CSV = Path(r"C:\UnityProjects\FYP\GameWebAPI\app\ml\Analytics\Preprocessed\session_features.csv")

pd.set_option("display.max_columns", None)
pd.set_option("display.width", None)
pd.set_option("display.max_colwidth", None)

def extract_session_id(file_path: Path) -> str:
    """
    Extract session id from filename like:
    turn_snapshots_20260406_204247.csv
    -> 20260406_204247
    """
    match = re.search(r"turn_snapshots_(\d{8}_\d{6})", file_path.stem)
    if match:
        return match.group(1)
    return file_path.stem


def safe_divide(numerator, denominator):
    """
    Safe divide for pandas series / scalars.
    Replaces 0 denominator with NaN first.
    """
    if isinstance(denominator, pd.Series):
        denominator = denominator.replace(0, np.nan)
    elif denominator == 0:
        denominator = np.nan
    return numerator / denominator


def preprocess_turn_snapshot_file(file_path: Path) -> dict | None:
    """
    Read one turn snapshot csv and return one session-level feature row.
    """
    try:
        turn_df = pd.read_csv(file_path)
    except Exception as e:
        print(f"[ERROR] Failed to read {file_path.name}: {e}")
        return None

    if turn_df.empty:
        print(f"[WARN] Skipping empty file: {file_path.name}")
        return None

    required_columns = [
        "Turn",
        "Gold",
        "Population",
        "TotalSupplyProvided",
        "AP",
        "APUsed",
        "UpgradeCount",
        "DemolishCount",
        "SmallHouseCount",
        "BigHouseCount",
        "SupplyCount",
        "ServiceCount",
        "FactoryCount",
        "RoadCount",
        "AverageSatisfactionIndex",
        "AveragePollutionIndex",
        "AverageServiceIndex",
        "HousesNearFactoryCount",
        "HousesWithoutServiceCount",
        "HousesLowSatisfactionCount",
        "TotalTaxIncome",
    ]

    missing = [col for col in required_columns if col not in turn_df.columns]
    if missing:
        print(f"[WARN] Skipping {file_path.name}, missing columns: {missing}")
        return None

    df = turn_df.copy()
    # hard limit below t15 here.. 
    target_df = df[df["Turn"] == 21].copy()
    df = df[df["Turn"] <= 16].copy()

    if df.empty:
        print(f"[WARN] Skipping {file_path.name}: no turns <= 15")
        return None

    if target_df.empty:
        print(f"[WARN] Skipping {file_path.name}: no Turn 20 row")
        return None
    
    target_row = target_df.iloc[0]

    # ---------- build/action derived columns ----------
    df["BuiltHouseCount"] = df["SmallHouseCount"].diff() + df["BigHouseCount"].diff()
    df["BuiltFactoryCount"] = df["FactoryCount"].diff()
    df["BuiltRoadCount"] = df["RoadCount"].diff()
    df["BuiltServiceCount"] = df["ServiceCount"].diff()
    df["BuiltSupplyCount"] = df["SupplyCount"].diff()

    # clamp negatives just in case demolish / weird data appears
    build_cols = [
        "BuiltHouseCount",
        "BuiltFactoryCount",
        "BuiltRoadCount",
        "BuiltServiceCount",
        "BuiltSupplyCount",
    ]
    for col in build_cols:
        df[col] = df[col].clip(lower=0)

    df["TotalBuildCount"] = (
        df["BuiltHouseCount"]
        + df["BuiltFactoryCount"]
        + df["BuiltRoadCount"]
        + df["BuiltServiceCount"]
        + df["BuiltSupplyCount"]
    )

    df["TurnActionCount"] = (
        df["UpgradeCount"]
        + df["DemolishCount"]
        + df["TotalBuildCount"]
    )

    df["ActionIntensity"] = safe_divide(df["APUsed"], df["AP"])
    df["UpgradeRatio"] = safe_divide(df["UpgradeCount"], df["TurnActionCount"])
    df["DemolishRatio"] = safe_divide(df["DemolishCount"], df["TurnActionCount"])
    df["BuildRatio"] = safe_divide(df["TotalBuildCount"], df["TurnActionCount"])

    df["BuildHouseRatio"] = safe_divide(df["BuiltHouseCount"], df["TotalBuildCount"])
    df["BuildFactoryRatio"] = safe_divide(df["BuiltFactoryCount"], df["TotalBuildCount"])
    df["BuildServiceRatio"] = safe_divide(df["BuiltServiceCount"], df["TotalBuildCount"])
    df["BuildSupplyRatio"] = safe_divide(df["BuiltSupplyCount"], df["TotalBuildCount"])
    df["BuildRoadRatio"] = safe_divide(df["BuiltRoadCount"], df["TotalBuildCount"])

    df["HousePerRoad"] = safe_divide(df["BuiltHouseCount"], df["BuiltRoadCount"])
    df["PopulationPerAP"] = safe_divide(df["Population"], df["APUsed"])
    df["GoldPerPopulation"] = safe_divide(df["Gold"], df["Population"])
    df["TaxIncomePerPopulation"] = safe_divide(df["TotalTaxIncome"], df["Population"])

    # ---------- risk / quality ----------
    total_houses = df["SmallHouseCount"] + df["BigHouseCount"]

    df["BadPlacementRate"] = df["HousesNearFactoryCount"] / total_houses.replace(0, np.nan)
    df["NoServiceRate"] = df["HousesWithoutServiceCount"] / total_houses.replace(0, np.nan)
    df["LowSatisfactionRate"] = df["HousesLowSatisfactionCount"] / total_houses.replace(0, np.nan)

    # ---------- growth ----------
    df["PopulationGrowth"] = df["Population"].diff()
    df["GoldGrowth"] = df["Gold"].diff()
    df["APGrowth"] = df["AP"].diff()
    df["SupplyGrowth"] = df["TotalSupplyProvided"].diff()
    df["SatisfactionGrowth"] = df["AverageSatisfactionIndex"].diff()
    df["ServiceGrowth"] = df["AverageServiceIndex"].diff()
    df["PollutionGrowth"] = df["AveragePollutionIndex"].diff()
    df["TaxIncomeGrowth"] = df["TotalTaxIncome"].diff()

    df = df.fillna(0)

    total_population_growth = df["Population"].iloc[-1] - df["Population"].iloc[0]
    total_gold_growth = df["Gold"].iloc[-1] - df["Gold"].iloc[0]
    total_supply_growth = df["TotalSupplyProvided"].iloc[-1] - df["TotalSupplyProvided"].iloc[0]
    total_ap_growth = df["AP"].iloc[-1] - df["AP"].iloc[0]
    total_tax_income_growth = df["TotalTaxIncome"].iloc[-1] - df["TotalTaxIncome"].iloc[0]

    supply_efficiency = (
        total_supply_growth / total_population_growth
        if total_population_growth != 0 else 0
    )

    df["SupplyRatio"] = safe_divide(df["TotalSupplyProvided"], df["Population"])
    avg_supply_ratio = df["SupplyRatio"].mean()
    min_supply_ratio = df["SupplyRatio"].min()

    service_to_house = df["BuiltServiceCount"].mean() / (df["BuiltHouseCount"].mean() + 1e-6)
    factory_to_house = df["BuiltFactoryCount"].mean() / (df["BuiltHouseCount"].mean() + 1e-6)

    # ---------- early game features (Turn 1-5) ----------
    early_df = df[df["Turn"] <= 5].copy()

    early_total_builds = early_df["TotalBuildCount"].sum()
    early_house_builds = early_df["BuiltHouseCount"].sum()
    early_service_builds = early_df["BuiltServiceCount"].sum()
    early_factory_builds = early_df["BuiltFactoryCount"].sum()

    used_service_early = int(early_service_builds > 0)
    used_factory_early = int(early_factory_builds > 0)

    early_service_ratio = early_service_builds / (early_total_builds + 1e-6)
    early_factory_ratio = early_factory_builds / (early_total_builds + 1e-6)

    early_service_to_house = early_service_builds / (early_house_builds + 1e-6)
    early_factory_to_house = early_factory_builds / (early_house_builds + 1e-6)

    session_features = {
        "session_id": extract_session_id(file_path),

        "avg_action_intensity": df["ActionIntensity"].mean(),
        "std_action_intensity": df["ActionIntensity"].std(),

        "avg_upgrade_ratio": df["UpgradeRatio"].mean(),
        "avg_demolish_ratio": df["DemolishRatio"].mean(),
        "avg_build_ratio": df["BuildRatio"].mean(),

        "avg_build_house_ratio": df["BuildHouseRatio"].mean(),
        "avg_build_factory_ratio": df["BuildFactoryRatio"].mean(),
        "avg_build_service_ratio": df["BuildServiceRatio"].mean(),
        "avg_build_supply_ratio": df["BuildSupplyRatio"].mean(),
        "avg_build_road_ratio": df["BuildRoadRatio"].mean(),

        "avg_house_per_turn": df["BuiltHouseCount"].mean(),
        "avg_factory_per_turn": df["BuiltFactoryCount"].mean(),
        "avg_service_per_turn": df["BuiltServiceCount"].mean(),
        "avg_supply_per_turn": df["BuiltSupplyCount"].mean(),
        "avg_road_per_turn": df["BuiltRoadCount"].mean(),

        "avg_gold_per_population": df["GoldPerPopulation"].mean(),
        "avg_tax_per_population": df["TaxIncomePerPopulation"].mean(),
        "avg_population_per_ap": df["PopulationPerAP"].mean(),
        "avg_house_per_road": df["HousePerRoad"].mean(),

        "avg_bad_placement": df["BadPlacementRate"].mean(),
        "avg_no_service": df["NoServiceRate"].mean(),
        "avg_low_satisfaction": df["LowSatisfactionRate"].mean(),

        "avg_population_growth": df["PopulationGrowth"].mean(),
        "avg_gold_growth": df["GoldGrowth"].mean(),
        "avg_supply_growth": df["SupplyGrowth"].mean(),
        "avg_ap_growth": df["APGrowth"].mean(),
        "avg_satisfaction_growth": df["SatisfactionGrowth"].mean(),
        "avg_service_growth": df["ServiceGrowth"].mean(),
        "avg_pollution_growth": df["PollutionGrowth"].mean(),
        "avg_tax_income_growth": df["TaxIncomeGrowth"].mean(),

        "std_population_growth": df["PopulationGrowth"].std(),
        "std_gold_growth": df["GoldGrowth"].std(),
        "std_supply_growth": df["SupplyGrowth"].std(),
        "std_satisfaction_growth": df["SatisfactionGrowth"].std(),
        "std_service_growth": df["ServiceGrowth"].std(),
        "std_pollution_growth": df["PollutionGrowth"].std(),

        "avg_satisfaction_index": df["AverageSatisfactionIndex"].mean(),
        "avg_service_index": df["AverageServiceIndex"].mean(),
        "avg_pollution_index": df["AveragePollutionIndex"].mean(),

        "total_population_growth": total_population_growth,
        "total_gold_growth": total_gold_growth,
        "total_supply_growth": total_supply_growth,
        "total_ap_growth": total_ap_growth,
        "total_tax_income_growth": total_tax_income_growth,
        "supply_efficiency": supply_efficiency,
        "avg_supply_ratio": avg_supply_ratio,
        "min_supply_ratio": min_supply_ratio,
        "service_to_house": service_to_house,
        "factory_to_house": factory_to_house,

        # early game feats
        "used_service_early": used_service_early,
        "used_factory_early": used_factory_early,
        "early_service_ratio": early_service_ratio,
        "early_factory_ratio": early_factory_ratio,
        "early_service_to_house": early_service_to_house,
        "early_factory_to_house": early_factory_to_house,

        "final_turn": df["Turn"].iloc[-1],
        "final_gold": df["Gold"].iloc[-1],
        "final_population": target_row["Population"],
        "final_supply": target_row["TotalSupplyProvided"],
        "final_ap": target_row["Population"],
        "final_satisfaction": target_row["AverageSatisfactionIndex"],
        "final_service": target_row["AverageServiceIndex"],
        "final_pollution": target_row["AveragePollutionIndex"],
        "final_tax_income": target_row["TotalTaxIncome"],
    }

    # replace NaN std from very short runs
    for key, value in session_features.items():
        if pd.isna(value):
            session_features[key] = 0

    return session_features


def preprocess_all_turn_snapshots(input_folder: Path, output_csv: Path | None = None) -> pd.DataFrame:
    """
    Read all turn_snapshots_*.csv under folder, preprocess each into 1 row,
    and combine into a single dataframe.
    """
    files = sorted(input_folder.glob("turn_snapshots_*.csv"))

    if not files:
        raise FileNotFoundError(f"No turn_snapshots_*.csv files found in: {input_folder}")

    all_rows = []

    for file_path in files:
        print(f"Processing: {file_path.name}")
        row = preprocess_turn_snapshot_file(file_path)
        if row is not None:
            all_rows.append(row)

    if not all_rows:
        raise ValueError("No valid turn snapshot files were successfully processed.")

    all_features_df = pd.DataFrame(all_rows)

    # optional: sort by session_id
    if "session_id" in all_features_df.columns:
        all_features_df = all_features_df.sort_values("session_id").reset_index(drop=True)

    if output_csv is not None:
        all_features_df.to_csv(output_csv, index=False)
        print(f"\nSaved combined features to: {output_csv}")

    return all_features_df

def preprocess_turn_snapshot_regression(df: pd.DataFrame) -> dict:
    df = df.drop_duplicates().copy()
    df = df.replace([np.inf, -np.inf], np.nan)

    df = df.sort_values("Turn").reset_index(drop=True)

    if len(df) < 2:
        return None  # not enough data to compute growth

    # ---------- derived ----------
    df["ActionIntensity"] = safe_divide(df["APUsed"], df["AP"])

    df["GoldGrowth"] = df["Gold"].diff()
    df["SupplyGrowth"] = df["TotalSupplyProvided"].diff()
    df["APGrowth"] = df["AP"].diff()
    df["SatisfactionGrowth"] = df["AverageSatisfactionIndex"].diff()
    df["ServiceGrowth"] = df["AverageServiceIndex"].diff()
    df["PollutionGrowth"] = df["AveragePollutionIndex"].diff()
    df["TaxIncomeGrowth"] = df["TotalTaxIncome"].diff()

    # ---------- clean ALL NaN here ----------
    df = df.fillna(0)

    # ---------- totals ----------
    total_population_growth = df["Population"].iloc[-1] - df["Population"].iloc[0]
    total_supply_growth = df["TotalSupplyProvided"].iloc[-1] - df["TotalSupplyProvided"].iloc[0]

    # ---------- aggregates ----------
    avg_action_intensity = df["ActionIntensity"].mean()
    avg_gold_growth = df["GoldGrowth"].mean()
    avg_supply_growth = df["SupplyGrowth"].mean()
    avg_ap_growth = df["APGrowth"].mean()
    avg_satisfaction_growth = df["SatisfactionGrowth"].mean()
    avg_service_growth = df["ServiceGrowth"].mean()
    avg_pollution_growth = df["PollutionGrowth"].mean()
    avg_tax_income_growth = df["TaxIncomeGrowth"].mean()

    supply_efficiency = (
        total_supply_growth / total_population_growth
        if total_population_growth != 0 else 0
    )

    input_features = {
        "avg_action_intensity": avg_action_intensity,
        "avg_gold_growth": avg_gold_growth,
        "avg_supply_growth": avg_supply_growth,
        "avg_ap_growth": avg_ap_growth,
        "avg_satisfaction_growth": avg_satisfaction_growth,
        "avg_service_growth": avg_service_growth,
        "avg_pollution_growth": avg_pollution_growth,
        "avg_tax_income_growth": avg_tax_income_growth,
        "supply_efficiency": supply_efficiency,
    }

    return input_features

def preprocess_turn_snapshot_cluster(df: pd.DataFrame) -> dict:
    df = df.drop_duplicates().copy()
    df = df.replace([np.inf, -np.inf], np.nan)

    df = df.sort_values("Turn").reset_index(drop=True)

    if len(df) < 2:
        return None

    # ---------- build counts ----------
    df["BuiltHouseCount"] = (
        df["SmallHouseCount"].diff() +
        df["BigHouseCount"].diff()
    )

    df["BuiltServiceCount"] = df["ServiceCount"].diff()
    df["BuiltFactoryCount"] = df["FactoryCount"].diff()

    # ---------- derived ----------
    df["ActionIntensity"] = safe_divide(df["APUsed"], df["AP"])
    df["PopulationGrowth"] = df["Population"].diff()
    df["GoldGrowth"] = df["Gold"].diff()
    df["SupplyGrowth"] = df["TotalSupplyProvided"].diff()
    df["SupplyRatio"] = safe_divide(df["TotalSupplyProvided"], df["Population"])

    # ---------- clean ALL NaN ----------
    df = df.fillna(0)

    # ---------- prevent negative builds (demolish issue) ----------
    df["BuiltHouseCount"] = df["BuiltHouseCount"].clip(lower=0)
    df["BuiltServiceCount"] = df["BuiltServiceCount"].clip(lower=0)
    df["BuiltFactoryCount"] = df["BuiltFactoryCount"].clip(lower=0)

    # ---------- early ----------
    early_df = df[df["Turn"] <= 5]

    early_service_builds = early_df["BuiltServiceCount"].sum()
    early_factory_builds = early_df["BuiltFactoryCount"].sum()

    # ---------- aggregates ----------
    avg_house_per_turn = df["BuiltHouseCount"].mean()

    service_to_house = safe_divide(
        df["BuiltServiceCount"].mean(),
        df["BuiltHouseCount"].mean()
    )

    factory_to_house = safe_divide(
        df["BuiltFactoryCount"].mean(),
        df["BuiltHouseCount"].mean()
    )

    avg_population_growth = df["PopulationGrowth"].mean()
    avg_gold_growth = df["GoldGrowth"].mean()
    avg_supply_ratio = df["SupplyRatio"].mean()
    avg_action_intensity = df["ActionIntensity"].mean()

    used_service_early = int(early_service_builds > 0)
    used_factory_early = int(early_factory_builds > 0)

    input_features = {
        "avg_action_intensity": avg_action_intensity,
        "avg_house_per_turn": avg_house_per_turn,
        "service_to_house": service_to_house,
        "factory_to_house": factory_to_house,
        "avg_population_growth": avg_population_growth,
        "avg_gold_growth": avg_gold_growth,
        "avg_supply_ratio": avg_supply_ratio,
        "used_service_early": used_service_early,
        "used_factory_early": used_factory_early,
    }

    return input_features

if __name__ == "__main__":
    all_features_df = preprocess_all_turn_snapshots(INPUT_FOLDER, OUTPUT_CSV)

    print("\n=== Combined Session-Level Features ===")
    print(all_features_df.head())
    print("\nShape:", all_features_df.shape)