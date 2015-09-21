Use ORTSigningPro_TEST

Declare @MobileClosingAssignmentID as int,
		@WindowStartDate as DateTimeOffset,
		@WindowEndDate as DateTimeOffset,
		@Cursor as Cursor,
		@RowNumber as int,
		@LastMobileClosingAssignmentID as int,
		@MobileClosingID as int,
		@Mode as int,
		@WindowMinuteIncrement as int,
		@CurrentDateTime as DateTimeOffset;

Set @Cursor = Cursor For (Select 
								MobileClosingAssignmentID,
								MobileClosingID
							From MobileClosingAssignment);

Set @RowNumber = 0;
Set @LastMobileClosingAssignmentID = 1;

/* -------------------- */
/* Parameters To Change */
/* -------------------- */

Set @Mode = 1;
Set @CurrentDateTime = GETDATE();
-- Mode of 1 will set the Window Start Date = Today's Date minus 1 day and Window End Date = Today's Date plus 3 days
-- Mode of 2 will set the actual window times with a 15 minute span between each

-- Only used if mode is a 2
Set @WindowMinuteIncrement = 15;

/* --------------------------- */
/* End of Parameters to Change */
/* --------------------------- */

Open @Cursor;
Fetch Next From @Cursor Into @MobileClosingAssignmentID, @MobileClosingID;

While @@FETCH_STATUS = 0
Begin
	Print 'MobileClosingAssignmentID';
	Print @MobileClosingAssignmentID;
	Print 'LastMobileClosingAssignmentID';
	Print @LastMobileClosingAssignmentID;

	Set @RowNumber =	(Select Top 1 A.Row From 
						(Select MobileClosingID, MobileClosingAssignmentID, ROW_NUMBER() OVER (PARTITION BY MobileClosingID Order By MobileClosingID) as 'Row'
						From MobileClosingAssignment Where MobileClosingID = @MobileClosingID) as A Where A.MobileClosingAssignmentID = @MobileClosingAssignmentID);

	Print 'RowNumber';
	Print @RowNumber;

	Set @WindowStartDate = Case	When @Mode = 1 Then CAST(CAST(DATEADD(DAY,-1, @CurrentDateTime) as date) as datetimeoffset) 
							When @RowNumber = 1 Then CAST( DATEADD(Hour,6,@CurrentDateTime) as datetimeoffset) 
							Else 
								DATEADD(MINUTE,@WindowMinuteIncrement,(Select TOP 1 WindowStartDate From MobileClosingAssignment 
								Where MobileClosingAssignmentID = @LastMobileClosingAssignmentID))
							End;

	Set @WindowEndDate = Case		When @Mode = 1 Then CAST(CAST(DATEADD(DAY,3, @CurrentDateTime) as date) as datetimeoffset) 
							When @RowNumber = 1 Then DATEADD(MINUTE, 15, CAST( DATEADD(Hour,6, @CurrentDateTime) as datetimeoffset))
							Else 
								DATEADD(MINUTE,@WindowMinuteIncrement,(Select TOP 1 WindowEndDate From MobileClosingAssignment 
								Where MobileClosingAssignmentID = @LastMobileClosingAssignmentID))
							End;

	Print 'WindowStartDate';
	Print @WindowStartDate;

	Print 'WindowEndDate';
	Print @WindowEndDate;

	Update MobileClosingAssignment 
		Set AcceptedDate = null,
			DeclinedDate = null,
			WindowStartDate = @WindowStartDate,
			WindowEndDate = @WindowEndDate
		Where MobileClosingAssignmentID = @MobileClosingAssignmentID

	Update MobileClosing
		Set VincaVendorID = 0,
			MobileClosingStatusCode = 0,
			AcceptedDateTime = null,
			DeclinedDateTime = null,
			RescindedDateTime = null,
			ConfirmedWithBorrowerDateTime = null,
			DocsDownloadedPrintedDateTime = null,
			InstructionsReviewedDateTime = null,
			ArrivedAtSigningLocationDateTime = null,
			CompletedDateTime = null,
			DocumentsScannedDateTime = null,
			VincaWorkAcceptedDateTime = null,
			DroppedDateTime = null,
			DeletedDateTime = null,
			VincaPaymentSubmittedDateTime = null,
			VincaPaymentAmount = 0.00,
			MobileClosingPaymentCode = 0
		Where MobileClosingID = @MobileClosingID

	Set @LastMobileClosingAssignmentID = @MobileClosingAssignmentID;

	Print 'LastMobileClosingAssignmentID';
	Print @LastMobileClosingAssignmentID;

	Fetch Next From @Cursor Into @MobileClosingAssignmentID, @MobileClosingID;
End

Close @Cursor;
DEALLOCATE @Cursor;