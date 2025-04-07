# Specification for Rename V0.1

Shell UX. 

	Main commands: 

	cd - change directory
	ls - list current directory
	select_where - select by following parameters: 
		filename
		date_created
		date_modified
		filesize
	After selection, filename will be stored in an array. 
	Usage: 
		select_where (
		filename="(FILENAME|REGEX...) | *",
		date_created="((<[=]|>[=])(epoch|TIME(non-utc)||.) | *", 
		date_modified="((<[=]|>[=])(epoch|TIME(non-utc)||.) | *", 
		filesize="((<[=]|>[=])(size+unit)||.) | *"
		)
	pending - shows the files to be edited. 
	substring - specify the range for editing. Default all. 
	Usage: 
		substring((<start>, [end(exclusive)], [id || sequential id]) OR ("reset", [id || sequential id]))
			start - the start of "will be edited sequence"
			end - the end of "will be edited sequence"
			id - the id of the pending file. Could be range or eval. 
            reset - reset keyword, the following id indicates the id you wanted to reset. 
	Eg. Usage: 
		substring(5) - Will only rename the file name string starting from the fifth character. 
		substring(3, 20) - *Will only rename the file name string in the range of the third character to the nineteenth character, exclusivity considered. *
		substring(3, 20, da325bc35ab33f) - * and with an id that is 92.
        substring(3, 20, 25-30) - * and with an id that is 25 to 30(exclusive)
                            >30 - * and with an id that is bigger than 30.
                            <80 - * and with an id that is smaller than 80.
	order_by() - order the array by the following: 
		[!]filename
		[!]date_created
		[!]date_modified
		[!]filesize
    id would not change by reordering the list.
    all() - equivalent to: select_where(*,*,*,*)
	clear - clear the pending. 
    cls - console clear. 
    
    all of the above is not actually functions and are parsed very stupidlly. skill issue. don't even wonder to chain them together and shit. 
	
