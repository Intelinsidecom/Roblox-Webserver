import React from "react";
import { createUseStyles } from "react-jss";
import LibraryInput from "./library/libraryInput";
import LibraryNavigation from "./library/libraryNavigation";
import LibraryResults from "./library/libraryResults";
import LibraryFilters from "./library/libraryFilters";

const useStyles = createUseStyles({
    LibraryContainer: {
        backgroundColor: 'var(--white-color)',
        padding: '2px 4px',
    },
})

const LibraryPage = props => {
    return <div>
        <div className='row mt-2'>
            <div className='col-12 col-md-8 col-lg-10' style={{marginLeft: 'auto'}}>
                <LibraryInput />
            </div>
        </div>
        <div className='row'>
            <div className='col-12 col-md-4 col-lg-2'>
                <div className='divider-right'>
                    <div className='pe-2'>
                        <LibraryNavigation />
                        <LibraryFilters />
                    </div>
                </div>
            </div>
            <div className='col-12 col-md-8 col-lg-10'>
                <LibraryResults />
            </div>
        </div>
    </div>
}

export default LibraryPage;
