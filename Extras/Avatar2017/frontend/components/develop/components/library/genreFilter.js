import { createUseStyles } from "react-jss";
import LibraryPageStore from "../../../../stores/libraryPage";

const useStyles = createUseStyles({
  label: {
    fontSize: '13px',
    paddingLeft: '4px',
  },
  header: {
    fontWeight: 600,
    fontSize: '16px',
  },
  allGenres: {
    paddingLeft: '5px',
    fontWeight: 600,
    marginBottom: 0,
    paddingBottom: 0,
  },
})

/**
 * @type {{genre: number, name: string}[]}
 */
export const genresList = [
  { genre: 0, name: 'All' },
  { genre: 13, name: 'Building' },
  { genre: 5, name: 'Horror' },
  { genre: 1, name: 'Town and City' },
  { genre: 11, name: 'Military' },
  { genre: 9, name: 'Comedy' },
  { genre: 2, name: 'Medieval' },
  { genre: 7, name: 'Adventure' },
  { genre: 3, name: 'Sci-Fi' },
  { genre: 6, name: 'Naval' },
  { genre: 14, name: 'FPS' },
  { genre: 15, name: 'RPG' },
  { genre: 8, name: 'Sports' },
  { genre: 4, name: 'Fighting' },
  { genre: 10, name: 'Western' },
];

const GenreFilter = props => {
  const store = LibraryPageStore.useContainer();
  const s = useStyles();
  return <div>
    <p className={s.header}>Genre</p>
    <p className={s.allGenres + ' cursor-pointer'} onClick={() => {
      store.setGenres([]);
    }}>All Genres</p>
    {
      genresList.map(v => {
        const id = 'catalog_genre_fitler_' + v.genre;
        return <p className='mb-0' key={id}>
          <input id={id} type='checkbox' checked={store.genres.includes(v.genre)} onChange={(c) => {
            if (c.currentTarget.checked === false) {
              store.setGenres(store.genres.filter(x => x !== v.genre));
            } else {
              store.setGenres([...store.genres, v.genre]);
            }
          }}></input>
          <label htmlFor={id} className={s.label}>{v.name}</label>
        </p>
      })
    }
    <div className='divider-top mt-4 mb-4 divider-light'></div>
    <input id={'show_unavailable_items'} type='checkbox' checked={true} onChange={(c) => {}} disabled={true} />
    <label htmlFor={'show_unavailable_items'} className={s.label} style={{fontSize: '11px', position: 'relative', top: '-3px'}}>Show unavailable items</label>
  </div>
}

export default GenreFilter;