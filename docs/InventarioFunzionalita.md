# Inventario delle funzionalità di Performance2021 (versione VB)

Elenco di controllo per la migrazione a **Performance2027**: ogni voce va ritrovata, funzionante, nella nuova versione.
Ricavato dalle pagine XAML (tab, menu, pulsanti, caselle, gruppi) della versione VB `v02 - 2026 10 13 14`.

**Limiti dell'estrazione automatica:** mancano i pulsanti e i menu creati nel codice, i menu contestuali, le colonne delle
tabelle e le scorciatoie da tastiera. Vanno aggiunti a mano qui sotto, man mano che si usano le funzioni ("Aggiunte a mano").

## Tab principali

| Tab | Controllo VB | Fase |
|---|---|---|
| File Load (con mappa) | MainWindow, UserControlParquetFinder, GestioneMapsui | 3 e 12 |
| Data Plots | UserControlTimePlotView, UserControlSciChartPlot | 5 |
| Summary: Visible Time Range Summary | UserControlTimeRangeSummary | 5 |
| Summary: Channels Highlight | UserControl_Highlight | 5 |
| Starts | UserControlStartExpedition | 11 |
| Upwind / Downwind / Reaching | UserControlStraightLineVmgUpwind / VmgDownwind / Reaching | 7 |
| StraightLine | UserControlStraightLine | 7 |
| Tacks / Gybes (TacksGybes nascosto) | UserControlPavarot | 9 |
| Crossover | UserControlCrossoverTwsTwa | 11 |
| XY Plots (Plot, Report Manager, 3D Plot, 360 Checks) | UserControlSciChartXYplot | 6 (3D: 14) |
| Feed Forward | UserControlFeedForward | 8 |
| Polars | UserControlPolarManager | 10 |
| Benchmarks | UserControlBenchmarks | 10 |

## Voci per finestra e controllo

### UserControl/UserControlFeedForward.xaml  (fase 8)

- [ ] GroupBox: Report
- [ ] Button: Save  -- Save the report in the profile (the filter is saved as a copy)
- [ ] Button: Save as...
- [ ] Button: Delete
- [ ] Button: Calculate
- [ ] Button: Print PDF  -- One page per Y channel (selected bands together) or one chart per band, see the Layout tab
- [ ] Button: Copy table  -- Copy the table of the selected Y channel
- [ ] TabItem: Analysis
- [ ] GroupBox: Data
- [ ] Button: Filters...
- [ ] GroupBox: Y: efficiency channels
- [ ] Button: Add
- [ ] Button: Remove
- [ ] Button: Up
- [ ] Button: Down
- [ ] GroupBox: Bands
- [ ] GroupBox: X bins
- [ ] GroupBox: Model
- [ ] TabItem: Layout
- [ ] GroupBox: Chart
- [ ] CheckBox: Pdf: selected bands together  -- Checked: one chart per Y channel with the bands selected in the table (Show). Unchecked: one chart per band.
- [ ] CheckBox: Show sparse bins  -- Show the samples and the means of the sparse bins (not used in the calculation) in the chart and in the pdf
- [ ] GroupBox: Axes
- [ ] GroupBox: Highlights  -- Colours of the cells to look at: feed forward by potential gain and reliability, sensitivity by explained share
- [ ] Button: All (this channel)  -- Select every band with an optimum for the Y channel shown
- [ ] Button: All channels  -- Select every band with an optimum for every Y channel
- [ ] Button: None  -- Deselect all the bands of the Y channel shown
- [ ] Button: Clear highlight  -- Remove the highlighted (bold) row: all the drawn bands get the same weight in the chart

### FiltriForm.xaml  (fase 6)

- [ ] Button: New
- [ ] Button: Duplicate
- [ ] Button: Delete
- [ ] Button: Save
- [ ] Button: Close

### RenameProfileForm.xaml  (fase 3)

- [ ] Button: Rename
- [ ] Button: Cancel

### ManeuverCurrentWindow.xaml  (fase 11)

- [ ] Button: Ricalcola
- [ ] Button: Copia negli appunti
- [ ] TabItem: Manovre
- [ ] TabItem: Per fascia di tempo
- [ ] TabItem: Per zona

### QualitySettingsForm.xaml  (fase 3)

- [ ] TabItem: General
- [ ] GroupBox: Sampling
- [ ] GroupBox: Performance reference
- [ ] GroupBox: Automatic periods
- [ ] TabItem: Channels
- [ ] TabItem: Score thresholds
- [ ] GroupBox: Environment Quality - max tolerance used (%)  -- Applies to the wind channels: TWS, TWD and TWA. Lower numbers make the top scores harder to earn.
- [ ] GroupBox: Attitude Quality - max tolerance used (%)  -- Applies to the handling channels: HDG, AWA, boat speed, heel and trim. Lower numbers make the top scores harder to earn.
- [ ] GroupBox: Performance Quality - max distance from local reference (% of target)  -- How far the boat speed sat from the typical rate of the surrounding minutes, on the same point of sail, in percentage points of polar target. The distance is taken in absolute value: running well above the local rate is an anomaly too, usually a gust, a helpful wave or a dirty reading, rather than a repeatable performance.
- [ ] Button: Reset Defaults
- [ ] Button: Save and Refresh  -- Save the settings and recalculate the quality channels
- [ ] Button: Save
- [ ] Button: Cancel

### InerziaForm.xaml  (fase 3 / 9)

- [ ] GroupBox: Default from the displacement
- [ ] GroupBox: Tau in use
- [ ] Button: Reset to default  -- Use the default tau from the displacement
- [ ] GroupBox: Estimate from tacks and gybes (Pavarot)
- [ ] Button: Estimate  -- Fit the speed recovery after every tack and gybe of the loaded dataset
- [ ] Button: Use the estimate  -- Use the median tau of the accepted manoeuvres
- [ ] Button: Save and Refresh  -- Save in the profile and recompute the Inertial channels
- [ ] Button: Cancel

### SeaStateNormForm.xaml  (fase 3)

- [ ] Button: Reset Defaults
- [ ] Button: Save
- [ ] Button: Cancel

### LeewayModelForm.xaml  (fase 3)

- [ ] RadioButton: Visible plot range  -- Fit only on the time range currently visible in the Data Plots
- [ ] RadioButton: Whole file
- [ ] CheckBox: Canard angle signed by tack  -- Tick if the canard angle channel is positive to starboard (it is then flipped on port tack so positive means windward)
- [ ] CheckBox: Keel cant signed by tack  -- Tick if the keel cant channel is positive to starboard (it is then flipped on port tack so positive means windward)
- [ ] CheckBox: Invert keel cant sign  -- Applied after the previous option: positive must mean the keel bulb moved toward windward
- [ ] Button: Evaluate  -- Error of the model against the reference with the current coefficients
- [ ] Button: Fit  -- Adapt the coefficients marked Fit to the reference series
- [ ] Button: Reset Defaults
- [ ] Button: Save and Refresh  -- Save the coefficients and recompute the Leeway Model channel
- [ ] Button: Save
- [ ] Button: Cancel

### UserControl/UserControlAb3D.xaml  (fase 14)

- [ ] Button: Export Image
- [ ] Button: Clear selection
- [ ] Button: Recreate data

### UserControl/UserControlCrossoverTwsTwa.xaml  (fase 11)

- [ ] CheckBox: Targets  -- Show Targets
- [ ] CheckBox: Awa  -- Show Awa Lines
- [ ] CheckBox: Aws  -- Show Aws Lines
- [ ] CheckBox: Vmc  -- Creates virtual points of the existing one, based on the vmc over every Twa
- [ ] CheckBox: Distribution  -- Show the Distribution chart instead of the TwsTwa
- [ ] Button: Refresh
- [ ] TabItem: TwaVsTws
- [ ] Button: Img  -- Copy Plot Image to Clipboard
- [ ] ToggleButton: Pdf  -- Collect Plot Images and make a pdf whith all of those on the button up
- [ ] Button: Y zmIn
- [ ] Button: Y zmOut
- [ ] TabItem: 3D View

### UserControl/ChannelSelectorAndOrderer.xaml  (fase da assegnare)

- [ ] Button: &lt;&lt;
- [ ] Button: &gt;&gt;

### UserControl/UserControlBenchmarks.xaml  (fase 10)

- [ ] Button: Refresh
- [ ] Button: Save
- [ ] Button: Reload
- [ ] Button: Import
- [ ] Button: Delete
- [ ] CheckBox: Draw Same Type
- [ ] CheckBox: Show

### UserControl/UserControlLiftAndDrag.xaml  (fase 10)

- [ ] Button: Calculate
- [ ] Button: Update

### UserControl/UserControlParquetFinder.xaml  (fase 3)

- [ ] Button: Search
- [ ] Button: Output Channels  -- Click To Select Channels to be exported
- [ ] Button: Add New
- [ ] Button: Copy To Clipboard

### UserControl/UserControlParquetFinderFilter.xaml  (fase 3)

- [ ] Button: S  -- Select from a list
- [ ] Button: S  -- Select from a list
- [ ] Button: Save
- [ ] Button: Canc
- [ ] Button: Del  -- Remove this filter

### UserControl/UserControlPolarManager.xaml  (fase 10)

- [ ] Button: Load Polar
- [ ] Button: Import Vpp Xls
- [ ] Button: Export To Faro
- [ ] Button: Test
- [ ] Button: Lift
- [ ] Button: ORC
- [ ] Button: Test
- [ ] TabItem: Polar Chart
- [ ] TabItem: Lift Table

### UserControl/UserControlPolarSelect.xaml  (fase 10)

- [ ] Button: Save
- [ ] Button: Cancel
- [ ] Button: Delete

### UserControl/UserControlTimeRangeSummary.xaml  (fase 5)

- [ ] Button: Refresh
- [ ] Button: Periods Check  -- Check visible time range periods
- [ ] Button: Pdf

### UserControl/UserControlSciChartXYplot.xaml  (fase 6)

- [ ] CheckBox: X Axis  -- Plot the selected channel derivative
- [ ] CheckBox: Y Axis  -- Plot the selected channel derivative
- [ ] Button: Refresh
- [ ] Button: Pdf
- [ ] MenuItem: Settings  -- Plot Settings
- [ ] CheckBox: Windward Leeward Function  -- When a channel name has port or stbd in the name and another channel with the other tack exists, stbd is treathd as the active side and port as the opposite
- [ ] CheckBox: X Axis Channel: plot the derivative  -- Plot the derivative of the X selected channel
- [ ] CheckBox: Y Axis Channel: plot the derivative  -- Plot the derivative of the Y selected channel
- [ ] CheckBox: Show Legend  -- Show Legend
- [ ] CheckBox: Horizontal
- [ ] CheckBox: Horizontal
- [ ] CheckBox: Vertical
- [ ] CheckBox: Vertical
- [ ] CheckBox: Plot X Distribution  -- Plot Distribution X Bars
- [ ] CheckBox: Plot Y Distribution  -- Plot Distribution Y Bars
- [ ] CheckBox: Vmc Twa AutoFilter  -- When VMC is selected as Y channel set the TWA fileter in the defined range of his value
- [ ] CheckBox: Vmc Twa AutoFilter  -- When VMC is selected as Y channel set the TWA fileter in the defined range of his value
- [ ] CheckBox: Show Targets  -- Show Targets
- [ ] CheckBox: Show Benchmarks  -- Show Benchmarks
- [ ] CheckBox: Plot Trend Lines  -- Plot Trend Lines
- [ ] CheckBox: Plot Mean Averages  -- Plot Mean Averages
- [ ] Button: Filters...  -- Open the Data Source & Filter window (named filters of the profile)
- [ ] CheckBox: Sailing State Filter
- [ ] CheckBox: YAxis ZeroValue Is NaN
- [ ] CheckBox: Apply Grouping Filter Percentage
- [ ] CheckBox: Apply Percentile Boundary Filter
- [ ] CheckBox: Min
- [ ] CheckBox: Max
- [ ] CheckBox: Min
- [ ] CheckBox: Max
- [ ] CheckBox: Export 360  -- Copy 360 to clipboard
- [ ] Button: AddToReport
- [ ] Button: OpenDataFolder
- [ ] Button: &lt;
- [ ] Button: &gt;
- [ ] TabItem: Plot
- [ ] MenuItem: Selection
- [ ] Button: TrLines  -- Copy Trend Lines Funtions and Tables to Clipboard
- [ ] Button: Stats  -- Copy Statistic Table to Clipboard
- [ ] Button: Table  -- Copy Full Data Table to Clipboard
- [ ] Button: Img  -- Copy Plot Image to Clipboard
- [ ] ToggleButton: Pdf  -- Collect Plot Images and make a pdf whith all of those on the button up
- [ ] Button: Y zmIn
- [ ] Button: Y zmOut
- [ ] Button: AddToReport
- [ ] Button: SaveMacro
- [ ] Button: ReportToPdf
- [ ] Button: Next
- [ ] TabItem: Report Manager
- [ ] Button: New Report
- [ ] Button: Clone Report
- [ ] Button: Export2Pdf
- [ ] Button: Top
- [ ] Button: Up
- [ ] Button: Dn
- [ ] Button: Btm
- [ ] Button: Del
- [ ] Button: Top
- [ ] Button: Up
- [ ] Button: Dn
- [ ] Button: Btm
- [ ] Button: Del
- [ ] Button: Show
- [ ] TabItem: 3D Plot
- [ ] Button: Refresh
- [ ] TabItem: 360 Checks
- [ ] Button: Refresh

### UserControl/UserControlSelFromList.xaml  (fase da assegnare)

- [ ] Button: X  -- Clear Search
- [ ] Button: Select
- [ ] Button: Cancel

### UserControl/UserControlChannelSelectorTwin.xaml  (fase da assegnare)

- [ ] Button: &gt;&gt;
- [ ] Button: &lt;&lt;
- [ ] Button: Top
- [ ] Button: Up
- [ ] Button: Dn
- [ ] Button: Btm
- [ ] Button: Del

### UserControl/UserControlPavarotAdvancedPlot.xaml  (fase 9)

- [ ] Button: SQ

### UserControl/UserControlStartExpedition.xaml  (fase 11)

- [ ] Button: Load Starts
- [ ] MenuItem: Tools  -- Plot Settings
- [ ] Button: Print Start Pdf
- [ ] Button: Print Race Report Pdf
- [ ] Button: Send Last Report by email
- [ ] Button: Export To Csv
- [ ] Button: Zoom To Start
- [ ] Button: Open Reports Folder
- [ ] Button: Export Csv  -- Copy Starts Table to Clipboard
- [ ] Button: ZoomTo  -- Zoom to start time range
- [ ] Button: Print Pdf  -- Current start pdf report
- [ ] Button: Prev
- [ ] Button: Next
- [ ] Button: Gun
- [ ] Button: -5m
- [ ] Button: -3m
- [ ] Button: -1m
- [ ] Button: +1m
- [ ] Button: LineCross

### UserControl/UserControlStraightLineVmgDownwind.xaml  (fase 7)

- [ ] Button: Refresh
- [ ] MenuItem: Period Selection
- [ ] CheckBox: Min
- [ ] CheckBox: Max
- [ ] CheckBox: Min
- [ ] CheckBox: Max
- [ ] CheckBox: Min
- [ ] CheckBox: Max
- [ ] Button: Mouse Mode: Selection
- [ ] MenuItem: Tools
- [ ] Button: Channels Manager  -- Select Channels To Show
- [ ] Button: Update Keys  -- update KEYS property of the selected periods
- [ ] Button: Set Key (selected)  -- assign a value to the KEY property of the periods selected on the charts (yellow in the list)
- [ ] Button: Delete selected  -- delete all the periods selected on the charts (yellow in the list)
- [ ] MenuItem: Reports
- [ ] Button: Make a pdf
- [ ] CheckBox: Print charts  -- Include the charts (waits until they are drawn)
- [ ] CheckBox: Print table  -- Include the data table by TWS
- [ ] CheckBox: Print periods list  -- Include the list of the periods (always the last section)
- [ ] CheckBox: Create table csv  -- Also create a csv file with the full data table, next to the pdf
- [ ] CheckBox: Apply XY Plots filter  -- Charts, tables and reports use only the samples that pass the filter set in the XY Plots tab
- [ ] Button: Make html report  -- Same sections as the pdf, in a single html file with the charts embedded
- [ ] CheckBox: Choose pdf/html reports channels  -- Ticked: a list of the channels on screen opens when you make a pdf or html report, to choose which ones to print (the choice is saved)
- [ ] Button: +
- [ ] Button: -

### UserControl/UserControlStraightLineReaching.xaml  (fase 7)

- [ ] Button: Refresh
- [ ] MenuItem: Period Selection
- [ ] CheckBox: Min
- [ ] CheckBox: Max
- [ ] CheckBox: Max
- [ ] CheckBox: Min
- [ ] CheckBox: Max
- [ ] CheckBox: Min
- [ ] CheckBox: Max
- [ ] Button: Mouse Mode: Selection
- [ ] MenuItem: Tools
- [ ] Button: Channels Manager  -- Select Channels To Show
- [ ] Button: Update Keys  -- update KEYS property of the selected periods
- [ ] Button: Set Key (selected)  -- assign a value to the KEY property of the periods selected on the charts (yellow in the list)
- [ ] Button: Delete selected  -- delete all the periods selected on the charts (yellow in the list)
- [ ] MenuItem: Reports
- [ ] Button: Make a pdf
- [ ] CheckBox: Print charts  -- Include the charts (waits until they are drawn)
- [ ] CheckBox: Print table  -- Include the data table by TWS
- [ ] CheckBox: Print periods list  -- Include the list of the periods (always the last section)
- [ ] CheckBox: Create table csv  -- Also create a csv file with the full data table, next to the pdf
- [ ] CheckBox: Apply XY Plots filter  -- Charts, tables and reports use only the samples that pass the filter set in the XY Plots tab
- [ ] Button: Make html report  -- Same sections as the pdf, in a single html file with the charts embedded
- [ ] CheckBox: Choose pdf/html reports channels  -- Ticked: a list of the channels on screen opens when you make a pdf or html report, to choose which ones to print (the choice is saved)
- [ ] Button: +
- [ ] Button: -

### UserControl/UserControlStraightLineVmgUpwind.xaml  (fase 7)

- [ ] Button: Refresh
- [ ] MenuItem: Period Selection
- [ ] CheckBox: Min
- [ ] CheckBox: Max
- [ ] CheckBox: Min
- [ ] CheckBox: Max
- [ ] CheckBox: Min
- [ ] CheckBox: Max
- [ ] Button: Mouse Mode: Selection
- [ ] MenuItem: Tools
- [ ] Button: Channels Manager  -- Select Channels To Show
- [ ] Button: Update Keys  -- update KEYS property of the selected periods
- [ ] Button: Set Key (selected)  -- assign a value to the KEY property of the periods selected on the charts (yellow in the list)
- [ ] Button: Delete selected  -- delete all the periods selected on the charts (yellow in the list)
- [ ] MenuItem: Reports
- [ ] Button: Make a pdf
- [ ] CheckBox: Print charts  -- Include the charts (waits until they are drawn)
- [ ] CheckBox: Print table  -- Include the data table by TWS
- [ ] CheckBox: Print periods list  -- Include the list of the periods (always the last section)
- [ ] CheckBox: Create table csv  -- Also create a csv file with the full data table, next to the pdf
- [ ] CheckBox: Apply XY Plots filter  -- Charts, tables and reports use only the samples that pass the filter set in the XY Plots tab
- [ ] Button: Make html report  -- Same sections as the pdf, in a single html file with the charts embedded
- [ ] CheckBox: Choose pdf/html reports channels  -- Ticked: a list of the channels on screen opens when you make a pdf or html report, to choose which ones to print (the choice is saved)
- [ ] Button: +
- [ ] Button: -

### UserControl/UserControlTimePlotView.xaml  (fase 5)

- [ ] MenuItem: Settings  -- Plot Settings
- [ ] Button: Sel Channels
- [ ] ToggleButton: Normalize  -- Force Absolute Value
- [ ] ToggleButton: Derivative  -- Force Absolute Value
- [ ] ToggleButton: Windward Leeward  -- When a channel has an omologue for the other tack shows the lleward if port is selected and the windward if is starboard
- [ ] ToggleButton: Single Y Axis  -- Use a common axis for all channels
- [ ] CheckBox: Min
- [ ] CheckBox: Max
- [ ] ToggleButton: Show Target  -- Show target if available
- [ ] ToggleButton: Show Background  -- Show background colored by tack
- [ ] Button: Copy To Clipboard  -- Copy Chart as Image to Clipboard
- [ ] Button: Delete  -- Delete Current Plot
- [ ] Button: Ext
- [ ] Button: Top  -- Move The Chart To The Top
- [ ] Button: Up  -- Move The Chart Up
- [ ] Button: Dn  -- Move The Chart Down
- [ ] Button: Btm  -- Move The Chart To The Bottom

### MainWindow.xaml  (fase 3 (scheletro, menu) e 12 (mappa))

- [ ] Button: ☰  -- All tabs
- [ ] TabItem: File Load
- [ ] Button: Load Data  -- Select or Load Selected Source File
- [ ] MenuItem: File Utilities  -- Plot Settings
- [ ] Button: Rename Profile  -- Change the boat name stored in the active profile
- [ ] Button: Boat Inertia  -- Time constant of the boat (default from the displacement, estimate from tacks and gybes) used by the Inertial channels
- [ ] Button: Export reference data  -- Migration to Performance2027: export all the channels (raw and math), periods and Feed Forward results of the loaded dataset, to verify the new application
- [ ] CheckBox: Inertial: old windows method  -- Checked: Inertial channels with the old windows (TWA averaged 15 to 5 s before, Sow averaged over the next seconds). Unchecked (default): equilibrium speed Sow + tau x dSow/dt with the current TWA. Saved in the profile.
- [ ] Button: Import Expedition Log File  -- Select Expedition Text File
- [ ] Button: Import FaRo Log File  -- Select FaRo File (txt,gz,bin)
- [ ] Button: Import Deckman Log File  -- Select Deckman Exported File
- [ ] Button: Import Fast Skipper Log Files  -- Select Fast Skipper Log Files
- [ ] Button: Import Events xml Sail File  -- Add Sail Usage Channels
- [ ] Button: Import SailShape Files  -- Import SailShape JIB and MAIN .csv files
- [ ] Button: Export To Csv  -- Export Loaded data to CSV
- [ ] Button: Export To Expedition  -- Export current file to a csv expedition like format
- [ ] Button: PowerZoneReport  -- Copy the power zone report to the clipboard
- [ ] Button: Compact Parquet  -- Make a compact version of loaded parquet files with data of periods only
- [ ] Button: Export to Parquet  -- Make a compact version of loaded parquet files with data of periods only including math channels
- [ ] Button: Set All Channels to Export  -- Set All channels as to be exported
- [ ] Button: Super CompactParquet  -- Make a compact version of loaded parquet files with data of periods and selected channels only
- [ ] Button: Open Data Folder  -- Open Data Folder
- [ ] Button: Open Current File Folder  -- Open Current File Folder
- [ ] Button: Select File  -- Select Source File
- [ ] TabItem: Data Plots
- [ ] MenuItem: Periods
- [ ] Button: Update SailingState  -- Update SailingState with different settings
- [ ] Button: Auto Periods  -- Loop into loaded data to automatically make periods
- [ ] Button: Check Visible  -- Check periods of the visible time range only
- [ ] Button: Del Visible  -- Delete all periods in the visible range
- [ ] Button: Quality Settings  -- Settings of the PerformanceQuality, EnvironmentQuality and AttitudeQuality math channels
- [ ] Button: Sea State Norm  -- Table of the expected Sea State for each True Wind Speed (SeaStateNorm_m and SeaStateNormDelta_m channels)
- [ ] Button: Leeway Model  -- Coefficients of the theoretical leeway model (Leeway Model channel) and their automatic fit to a reference leeway
- [ ] Button: Import Test  -- Import Test
- [ ] Button: Export Periods  -- Export periods to prd file
- [ ] Button: UpdateKeys  -- Update periods Keys property of the visible range
- [ ] Button: UpdateRace  -- Update periods RaceName property of the visible range
- [ ] Button: UpdateLeg  -- Update periods LegName property of the visible range
- [ ] Button: KeysToRaceLeg  -- Split existing Keys description to Race Leg descriptions
- [ ] MenuItem: Tools
- [ ] Button: Refresh Plots  -- Rebuild the plot series from the current channel values
- [ ] Button: Meteo Report  -- Create Meteo Report and copy it to the clipboard
- [ ] Button: Quick Stats  -- Selected period percentage of upwind, downwind and reaching
- [ ] Button: Custom  -- Custom Function
- [ ] Button: Corrections  -- Apply Corrections
- [ ] Button: Test Stats  -- Test statistics
- [ ] MenuItem: Current Utilities
- [ ] Button: Set Visible as 1st segment  -- Use the visible range as the first segment of the current calculation
- [ ] Button: Set Visible as 2nd segment  -- Use the visible range as the second segment of the current calculation
- [ ] Button: Check result  -- Compute the current from the two segments and copy the result to the clipboard
- [ ] Button: Long Range Auto Check  -- Analyze the current over the whole visible range automatically
- [ ] Button: PedStats
- [ ] ToggleButton: R,B&A  -- Range, Bearing and Angles
- [ ] Button: All  -- Zoom to the whole track
- [ ] ToggleButton: Sel  -- On: show only the displayed period. Off: show the whole track
- [ ] Button: In  -- Zoom IN
- [ ] Button: Out  -- Zoom OUT
- [ ] Button: Chart
- [ ] ToggleButton: Hz  -- Draw track 1 or 20 Hz
- [ ] Button: Gpx  -- Export Waypoint to Gpx
- [ ] Button: Pop  -- Open the map in a resizable window
- [ ] Button: +
- [ ] Button: -
- [ ] TabItem: Summary
- [ ] TabItem: Visible Time Range Summary
- [ ] TabItem: Channels Highlight
- [ ] TabItem: Starts
- [ ] TabItem: Upwind
- [ ] TabItem: Downwind
- [ ] TabItem: Reaching
- [ ] TabItem: StraightLine
- [ ] TabItem: TacksGybes
- [ ] TabItem: Tacks
- [ ] TabItem: Gybes
- [ ] TabItem: Crossover
- [ ] TabItem: XY Plots
- [ ] TabItem: Feed Forward
- [ ] TabItem: Polars
- [ ] TabItem: Benchmarks

### UserControl/UserControlPavarot.xaml  (fase 9)

- [ ] MenuItem: Period Selection
- [ ] CheckBox: Tacks
- [ ] CheckBox: Gybes
- [ ] CheckBox: Stbd Entry
- [ ] CheckBox: Port Entry
- [ ] CheckBox: Min
- [ ] CheckBox: Max
- [ ] Button: Refresh
- [ ] MenuItem: Settings  -- Manoueuver Settings
- [ ] Button: Recalc
- [ ] MenuItem: Tools
- [ ] Button: Add Chart
- [ ] Button: Pdf Report  -- Export Pdf Reports
- [ ] Button: PedStats  -- Export to Clipboard selected manoeuvers pedestals statistics
- [ ] Button: Update Keys  -- update KEYS property of the selected periods
- [ ] ToggleButton: WheelZoom  -- Toggle mouse wheel zoom On/Off
- [ ] ToggleButton: GrByTack  -- Show Plots with data aggregated by tack
- [ ] ToggleButton: GrByK  -- Show Plots with data aggregated by period keys
- [ ] ToggleButton: ClByTk  -- Show Plots with data colored by entry tack
- [ ] Button: Recalculate
- [ ] Button: Add Chart
- [ ] Button: Pdf  -- Export Pdf Reports
- [ ] Button: Bazzo  -- Export Bazzos File
- [ ] Button: PedStats  -- Export to Clipboard selected manoeuvers pedestals statistics
- [ ] ToggleButton: WheelZoom  -- Toggle mouse wheel zoom On/Off
- [ ] Button: UpdateKeys  -- update KEYS property of the selected periods
- [ ] TabItem: Basic
- [ ] Button: +
- [ ] Button: -
- [ ] TabItem: Advanced

### UserControl/UserControlPavarotPlot.xaml  (fase 9)

- [ ] Button: Extents
- [ ] Button: Yauto
- [ ] Button: Yin
- [ ] Button: Yout
- [ ] Button: Sel
- [ ] Button: Top
- [ ] Button: Up
- [ ] Button: Dn
- [ ] Button: Btm
- [ ] Button: Del
- [ ] ToggleButton: Sgn
- [ ] ToggleButton: Der
- [ ] ToggleButton: Smt  -- Smooth the delta channel curves

### UserControl/UserControlStraightLine.xaml  (fase 7)

- [ ] Button: Refresh
- [ ] MenuItem: Period Selection
- [ ] CheckBox: Min
- [ ] CheckBox: Max
- [ ] CheckBox: Min
- [ ] CheckBox: Max
- [ ] Button: Mouse Mode: Selection
- [ ] MenuItem: Tools
- [ ] Button: Channels Manager  -- Select Channels To Show
- [ ] Button: Update Keys  -- update KEYS property of the selected periods
- [ ] Button: Set Key (selected)  -- assign a value to the KEY property of the periods selected on the charts (yellow in the list)
- [ ] Button: Delete selected  -- delete all the periods selected on the charts (yellow in the list)
- [ ] MenuItem: Reports
- [ ] Button: Make a pdf
- [ ] CheckBox: Print charts  -- Include the charts (waits until they are drawn)
- [ ] CheckBox: Print table  -- Include the data table by TWS
- [ ] CheckBox: Print periods list  -- Include the list of the periods (always the last section)
- [ ] CheckBox: Create table csv  -- Also create a csv file with the full data table, next to the pdf
- [ ] Button: Make html report  -- Same sections as the pdf, in a single html file with the charts embedded
- [ ] CheckBox: Choose pdf/html reports channels  -- Ticked: a list of the channels on screen opens when you make a pdf or html report, to choose which ones to print (the choice is saved)
- [ ] Button: +
- [ ] Button: -

### UserControl/UserControl_ImportaPeriodi.xaml  (fase 3)

- [ ] Button: SelectFile
- [ ] Button: Import
- [ ] Button: Cancel

### UserControl/UserControl_InputBox.xaml  (fase da assegnare)

- [ ] Button: OK
- [ ] Button: cancel

### UserControl/UserControl_Highlight.xaml  (fase 5)

- [ ] Button: Refresh Time Range
- [ ] Button: Refresh by Periods
- [ ] Button: Up Channels
- [ ] Button: Dn Channels
- [ ] Button: Select Filter Channel
- [ ] Button: Print Pdf
- [ ] TabItem: Genaral
- [ ] TabItem: Up PortAndStbd
- [ ] TabItem: Up Port Only
- [ ] TabItem: Up Stbd Only
- [ ] TabItem: Dn PortAndStbd
- [ ] TabItem: Dn Port Only
- [ ] TabItem: Dn Stbd Only

### Finestre e controlli senza voci nel XAML (pulsanti creati nel codice o solo visualizzazione)

- [ ] LoadingForm.xaml  (fase 3)
- [ ] SailList.xaml  (fase 11)
- [ ] SelectProfileForm.xaml  (fase 3)
- [ ] UserControl/SailShape.xaml  (fase 11)
- [ ] UserControl/UserControl360Checks.xaml  (fase 10)
- [ ] UserControl/UserControlExportHtml.xaml  (fase 7)
- [ ] UserControl/UserControlSciChartPlot.xaml  (fase 5)
- [ ] UserControl/PeriodManagerView.xaml  (fase 3)
- [ ] UserControl/ChannelPicker.xaml  (fase da assegnare)
- [ ] UserControl/UserControlNumericUpDown.xaml  (fase da assegnare)
- [ ] UserControl/UserControlStraightLinePlotContainer.xaml  (fase 7)
- [ ] UserControl/UserControlStraightLineStandardPlot.xaml  (fase 7)

## Aggiunte a mano

- [ ] (funzioni non presenti nell'elenco automatico: menu contestuali, scorciatoie, colonne, report…)
